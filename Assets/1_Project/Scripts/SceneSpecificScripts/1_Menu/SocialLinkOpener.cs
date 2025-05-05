using UnityEngine;

public class SocialLinkOpener : MonoBehaviour
{
    [Header("Social Media Links")]
    //public string twitterUrl = "https://twitter.com/YourGameHandle";
    //public string instagramUrl = "https://instagram.com/YourGameHandle";
    public string steamStoreUrl = "https://store.steampowered.com/app/3715890";
    public string discordUrl = "https://discord.gg/Ss9wKNbm";
    public string youtubeUrl = "https://youtube.com/YourChannel";
    
    [Header("Email Contact")]
    public string gmailAddress = "friendsandfangsgame@gmail.com";


    //public void fn_OpenTwitter() => Application.OpenURL(twitterUrl);
    //public void fn_OpenInstagram() => Application.OpenURL(instagramUrl);
    public void fn_OpenSteamStore() => Application.OpenURL(steamStoreUrl);
    public void fn_OpenDiscord() => Application.OpenURL(discordUrl);
    public void fn_OpenYouTube() => Application.OpenURL(youtubeUrl);
    public void fn_OpenGmail() => Application.OpenURL($"mailto:{gmailAddress}");
}
