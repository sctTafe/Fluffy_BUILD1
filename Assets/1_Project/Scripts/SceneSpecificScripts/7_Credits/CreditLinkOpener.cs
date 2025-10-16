using UnityEngine;

public class CreditLinkOpener : MonoBehaviour
{
    [Header("Social Media Links")]
    public string scottURL = "https://www.scottmakesgames.com";
    public string augustURL = "";
    public string rylanURL = "https://rylanha.wixsite.com/portfolio";
    public string braedonURL = "";
    public string leahURL = "https://mmomoruu.my.canva.site/portfolio";
    public string milaURL = "https://www.instagram.com/thlurp";
    public string izaacURL = "https://linkedin.com/in/izaac-st-pierre";
    public string nathanURL = "https://www.linkedin.com/in/nathan-harrison-a71825236/";


    //public void fn_OpenTwitter() => Application.OpenURL(twitterUrl);
    //public void fn_OpenInstagram() => Application.OpenURL(instagramUrl);

    public void fn_ScottPage() => Application.OpenURL(scottURL);
    public void fn_AugustPage() => Application.OpenURL(augustURL);
    public void fn_RylanPage() => Application.OpenURL(rylanURL);
    public void fn_BraedonPage() => Application.OpenURL(braedonURL);
	public void fn_LeahPage() => Application.OpenURL(leahURL);
	public void fn_MilaPage() => Application.OpenURL(milaURL);
	public void fn_IzzacPage() => Application.OpenURL(izaacURL);
    public void fn_NathanPage() => Application.OpenURL(nathanURL);
}
