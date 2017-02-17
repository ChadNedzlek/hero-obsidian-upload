using System.Collections.ObjectModel;
using System.Windows;

namespace ObsidianUpload
{
	public class CampaignInfo : DependencyObject
	{
		public static readonly DependencyProperty NameProperty = DependencyProperty.Register(
			"Name",
			typeof(string),
			typeof(CampaignInfo),
			new PropertyMetadata(default(string)));

		public string Name
		{
			get { return (string)GetValue(NameProperty); }
			set { SetValue(NameProperty, value); }
		}

		public static readonly DependencyProperty IdProperty = DependencyProperty.Register(
			"Id",
			typeof(string),
			typeof(CampaignInfo),
			new PropertyMetadata(default(string)));

		public string Id
		{
			get { return (string)GetValue(IdProperty); }
			set { SetValue(IdProperty, value); }
		}

		public static readonly DependencyProperty CharactersProperty = DependencyProperty.Register(
			"Characters",
			typeof(ObservableCollection<CharacterInfo>),
			typeof(CampaignInfo),
			new PropertyMetadata(default(ObservableCollection<CharacterInfo>)));

		public ObservableCollection<CharacterInfo> Characters
		{
			get { return (ObservableCollection<CharacterInfo>)GetValue(CharactersProperty); }
			set { SetValue(CharactersProperty, value); }
		}
	}
}