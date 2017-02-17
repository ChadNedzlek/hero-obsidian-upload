using System.Windows;

namespace ObsidianUpload
{
	public class CharacterInfo : DependencyObject
	{
		public static readonly DependencyProperty NameProperty = DependencyProperty.Register(
			"Name",
			typeof(string),
			typeof(CharacterInfo),
			new PropertyMetadata(default(string)));

		public string Name
		{
			get { return (string)GetValue(NameProperty); }
			set { SetValue(NameProperty, value); }
		}

		public static readonly DependencyProperty IdProperty = DependencyProperty.Register(
			"Id",
			typeof(string),
			typeof(CharacterInfo),
			new PropertyMetadata(default(string)));

		public string Id
		{
			get { return (string)GetValue(IdProperty); }
			set { SetValue(IdProperty, value); }
		}

		public static readonly DependencyProperty PlayerProperty = DependencyProperty.Register(
			"Player",
			typeof(string),
			typeof(CharacterInfo),
			new PropertyMetadata(default(string)));

		public string Player
		{
			get { return (string)GetValue(PlayerProperty); }
			set { SetValue(PlayerProperty, value); }
		}

		public static readonly DependencyProperty CampaignIdProperty = DependencyProperty.Register(
			"CampaignId",
			typeof(string),
			typeof(CharacterInfo),
			new PropertyMetadata(default(string)));

		public string CampaignId
		{
			get { return (string)GetValue(CampaignIdProperty); }
			set { SetValue(CampaignIdProperty, value); }
		}
	}
}