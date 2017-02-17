using System.Windows;

namespace ObsidianUpload
{
	public class CharacterSummary : DependencyObject
	{
		public static readonly DependencyProperty ClassLevelsProperty = DependencyProperty.Register(
			"ClassLevels",
			typeof(string),
			typeof(CharacterSummary),
			new PropertyMetadata(default(string)));

		public string ClassLevels
		{
			get { return (string)GetValue(ClassLevelsProperty); }
			set { SetValue(ClassLevelsProperty, value); }
		}

		public static readonly DependencyProperty NameProperty = DependencyProperty.Register(
			"Name",
			typeof(string),
			typeof(CharacterSummary),
			new PropertyMetadata(default(string)));

		public string Name
		{
			get { return (string)GetValue(NameProperty); }
			set { SetValue(NameProperty, value); }
		}

		public static readonly DependencyProperty RaceProperty = DependencyProperty.Register(
			"Race",
			typeof(string),
			typeof(CharacterSummary),
			new PropertyMetadata(default(string)));

		public string Race
		{
			get { return (string)GetValue(RaceProperty); }
			set { SetValue(RaceProperty, value); }
		}

		public static readonly DependencyProperty AlignmentProperty = DependencyProperty.Register(
			"Alignment",
			typeof(string),
			typeof(CharacterSummary),
			new PropertyMetadata(default(string)));

		public string Alignment
		{
			get { return (string)GetValue(AlignmentProperty); }
			set { SetValue(AlignmentProperty, value); }
		}

		public static readonly DependencyProperty GenderProperty = DependencyProperty.Register(
			"Gender",
			typeof(string),
			typeof(CharacterSummary),
			new PropertyMetadata(default(string)));

		public string Gender
		{
			get { return (string)GetValue(GenderProperty); }
			set { SetValue(GenderProperty, value); }
		}
	}
}