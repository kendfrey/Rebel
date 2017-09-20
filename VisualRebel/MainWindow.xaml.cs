using Rebel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace VisualRebel
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		Regex split = new Regex(@"(?<=(?<!\\)(?:\\\\)*)/", RegexOptions.Compiled);
		bool formatting = false;
		Brush lightRed = new SolidColorBrush(Color.FromRgb(0xFF, 0xBF, 0xBF));
		Brush lightYellow = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xBF));
		Brush lightGreen = new SolidColorBrush(Color.FromRgb(0xBF, 0xFF, 0xBF));
		BlockingCollection<string> inputQueue;
		AutoResetEvent stepContinue;
		RebelProgram program;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void rtbCode_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (formatting)
			{
				return;
			}
			formatting = true;
			TextRange textRange = new TextRange(rtbCode.Document.ContentStart, rtbCode.Document.ContentEnd);
			textRange.ClearAllProperties();
			List<Tuple<TextPointer, TextPointer>> delimiters = new List<Tuple<TextPointer, TextPointer>>();
			foreach (Match match in split.Matches(textRange.Text))
			{
				delimiters.Add(new Tuple<TextPointer, TextPointer>(
					textRange.Start.GetPositionAtOffset(match.Index),
					textRange.Start.GetPositionAtOffset(match.Index + match.Length)
					));
			}
			new TextRange(textRange.Start, delimiters.Count > 0 ? delimiters[0].Item1 : textRange.End)
				.ApplyPropertyValue(TextElement.BackgroundProperty, lightYellow);
			for (int i = 0; i < delimiters.Count; i++)
			{
				new TextRange(delimiters[i].Item2, delimiters.Count > i + 1 ? delimiters[i + 1].Item1 : textRange.End)
					.ApplyPropertyValue(TextElement.BackgroundProperty, i % 2 == 0 ? lightRed : lightGreen);
			}
			NoWrap(rtbCode);
			formatting = false;
		}

		private void rtbData_TextChanged(object sender, TextChangedEventArgs e)
		{
			NoWrap(rtbData);
		}

		private void rtbProgram_TextChanged(object sender, TextChangedEventArgs e)
		{
			NoWrap(rtbProgram);
		}

		private void rtbConsole_TextChanged(object sender, TextChangedEventArgs e)
		{
			rtbConsole.ScrollToEnd();
		}

		private void rtbInput_TextChanged(object sender, TextChangedEventArgs e)
		{
			TextRange textRange = new TextRange(rtbInput.Document.ContentStart, rtbInput.Document.ContentEnd);
			textRange.ClearAllProperties();
			NoWrap(rtbInput);
		}

		private void NoWrap(RichTextBox rtb)
		{
			rtb.Document.PageWidth = new FormattedText(
				new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd).Text,
				CultureInfo.CurrentCulture,
				rtb.FlowDirection,
				new Typeface(rtb.FontFamily.Source),
				rtb.FontSize,
				Brushes.Black
				).Width +
				rtb.Document.PagePadding.Left +
				rtb.Document.PagePadding.Right;
		}

		private void btnStartStep_Click(object sender, RoutedEventArgs e)
		{
			Start(true);
		}

		private void btnStartRun_Click(object sender, RoutedEventArgs e)
		{
			Start(false);
		}

		private void btnStep_Click(object sender, RoutedEventArgs e)
		{
			stepContinue.Set();
			btnStep.Visibility = Visibility.Collapsed;
			btnRun.Visibility = Visibility.Collapsed;
			btnPause.Visibility = Visibility.Visible;
			btnStop.Visibility = Visibility.Visible;
		}

		private void btnRun_Click(object sender, RoutedEventArgs e)
		{
			program.DebugMode = false;
			stepContinue.Set();
			btnStep.Visibility = Visibility.Collapsed;
			btnRun.Visibility = Visibility.Collapsed;
			btnPause.Visibility = Visibility.Visible;
			btnStop.Visibility = Visibility.Visible;
		}

		private void btnPause_Click(object sender, RoutedEventArgs e)
		{
			program.DebugMode = true;
		}

		private void btnStop_Click(object sender, RoutedEventArgs e)
		{
			program.Stop = true;
			stepContinue.Set();
		}

		private void Start(bool debugMode)
		{
			inputQueue = new BlockingCollection<string>();
			stepContinue = new AutoResetEvent(false);
			new TextRange(rtbConsole.Document.ContentStart, rtbConsole.Document.ContentEnd).Text = "";
			new TextRange(rtbInput.Document.ContentStart, rtbInput.Document.ContentEnd).Text = "";
			RunProgramParameters param = new RunProgramParameters();
			param.Program = new TextRange(rtbCode.Document.ContentStart, rtbCode.Document.ContentEnd).Text.TrimEnd('\r', '\n');
			param.DebugMode = debugMode;
			param.IO = new DebuggerIO(inputQueue, EchoToConsole, Dispatcher);
			Thread thread = new Thread(RunProgram);
			thread.IsBackground = true;
			thread.Start(param);
			btnStartStep.Visibility = Visibility.Collapsed;
			btnStartRun.Visibility = Visibility.Collapsed;
			btnPause.Visibility = Visibility.Visible;
			btnStop.Visibility = Visibility.Visible;
		}

		private void RunProgram(object obj)
		{
			try
			{
				RunProgramParameters param = obj as RunProgramParameters;
				program = new RebelProgram(param.Program, param.IO);
				program.DebugMode = param.DebugMode;
				program.DebugStep += program_DebugStep;
				program.Run();
				Dispatcher.Invoke(ProgramDone);
			}
			catch (Exception ex)
			{
				Dispatcher.Invoke(() => MessageBox.Show(ex.Message));
			}
		}

		private void program_DebugStep(object sender, DebugStepEventArgs e)
		{
			Dispatcher.Invoke(new Action<object, DebugStepEventArgs>(DebugStep), sender, e);
		}

		private void DebugStep(object sender, DebugStepEventArgs e)
		{
			Paragraph oldData = new Paragraph();
			TextRange oldDataText = new TextRange(oldData.ContentStart, oldData.ContentEnd);
			oldDataText.Text = e.OldData;
			new TextRange(oldDataText.Start.GetPositionAtOffset(e.ReplacementStart), oldDataText.Start.GetPositionAtOffset(e.ReplacementStart + e.OldReplacementLength))
				.ApplyPropertyValue(TextElement.BackgroundProperty, lightRed);
			Paragraph newData = new Paragraph();
			TextRange newDataText = new TextRange(newData.ContentStart, newData.ContentEnd);
			newDataText.Text = e.NewData;
			new TextRange(newDataText.Start.GetPositionAtOffset(e.ReplacementStart), newDataText.Start.GetPositionAtOffset(e.ReplacementStart + e.NewReplacementLength))
				.ApplyPropertyValue(TextElement.BackgroundProperty, lightGreen);
			rtbData.Document.Blocks.Clear();
			rtbData.Document.Blocks.Add(oldData);
			rtbData.Document.Blocks.Add(newData);
			rtbProgram.Document.Blocks.Clear();
			foreach (Tuple<Regex, string> pair in (sender as RebelProgram).Pairs)
			{
				Paragraph regex = new Paragraph();
				regex.Inlines.Add(pair.Item1.ToString());
				Paragraph replacement = new Paragraph();
				replacement.Inlines.Add(pair.Item2);
				if (pair == e.ReplacementPair)
				{
					regex.Inlines.FirstInline.Background = lightRed;
					replacement.Inlines.FirstInline.Background = lightGreen;
				}
				rtbProgram.Document.Blocks.Add(regex);
				rtbProgram.Document.Blocks.Add(replacement);
			}
			e.Continue = stepContinue;
			btnStep.Visibility = Visibility.Visible;
			btnRun.Visibility = Visibility.Visible;
			btnPause.Visibility = Visibility.Collapsed;
			btnStop.Visibility = Visibility.Collapsed;
		}

		private void ProgramDone()
		{
			inputQueue.Dispose();
			inputQueue = null;
			btnStartStep.Visibility = Visibility.Visible;
			btnStartRun.Visibility = Visibility.Visible;
			btnPause.Visibility = Visibility.Collapsed;
			btnStop.Visibility = Visibility.Collapsed;
			rtbData.Document.Blocks.Clear();
			rtbProgram.Document.Blocks.Clear();
			rtbConsole.Document.Blocks.Clear();
			rtbInput.Document.Blocks.Clear();
		}

		private void EchoToConsole(string s)
		{
			new TextRange(rtbConsole.Document.ContentStart, rtbConsole.Document.ContentEnd).Text += s;
		}

		private void rtbInput_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				TextRange textRange = new TextRange(rtbInput.Document.ContentStart, rtbInput.Document.ContentEnd);
				if (inputQueue != null)
				{
					inputQueue.Add(textRange.Text.Trim('\r', '\n'));
				}
				textRange.Text = "";
			}
		}

		class RunProgramParameters
		{
			public string Program;
			public bool DebugMode;
			public DebuggerIO IO;
		}
	}
}
