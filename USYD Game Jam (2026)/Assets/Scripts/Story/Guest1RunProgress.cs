using UnityEngine;

public static class Guest1RunProgress
{
    private const string LobbyCompletedKey = "HotelHunger.Guest1.LobbyCompleted";
    private const string DepartureCartChosenKey = "HotelHunger.Guest1.DepartureCartChosen";
    private const string SuitcaseCollectedKey = "HotelHunger.Guest1.SuitcaseCollected";

    public static bool LobbyCompleted
    {
        get => GetFlag(LobbyCompletedKey);
        set => SetFlag(LobbyCompletedKey, value);
    }

    public static bool DepartureCartChosen
    {
        get => GetFlag(DepartureCartChosenKey);
        set => SetFlag(DepartureCartChosenKey, value);
    }

    public static bool SuitcaseCollected
    {
        get => GetFlag(SuitcaseCollectedKey);
        set => SetFlag(SuitcaseCollectedKey, value);
    }

    public static void ClearAll()
    {
        PlayerPrefs.DeleteKey(LobbyCompletedKey);
        PlayerPrefs.DeleteKey(DepartureCartChosenKey);
        PlayerPrefs.DeleteKey(SuitcaseCollectedKey);
        PlayerPrefs.Save();
    }

    private static bool GetFlag(string key)
    {
        return PlayerPrefs.GetInt(key, 0) == 1;
    }

    private static void SetFlag(string key, bool value)
    {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
        PlayerPrefs.Save();
    }
}
