using UnityEngine;
using Unity.Entities;

namespace Beacon
{

    #region Debug Events
    public class ForceDayEvent : Event
    {
    }
    public class ForceNightEvent : Event
    {
    }
    public class ForceEndEvent : Event
    {
    }
    public class ForceWinEvent : Event
    {
    }
    public class ForceGameOverEvent : Event
    {
    }
    #endregion

    #region GameManager Events
    public class GameMenuEvent : Event
    {
    }
    public class GamePlayEvent : Event
    {
    }
    public class GamePlayDayEvent : Event
    {
    }
    public class GamePlayNightEvent : Event
    {
    }
    public class GamePauseEvent : Event
    {
    }
    public class GameOverEvent : Event
    {
    }
    public class GameVictoryEvent : Event
    {
    }
    #endregion

    #region WaveManager Events
    public class WaveStartedEvent : Event
    {
    }
    public class WaveEndedEvent : Event
    {
    }
    #endregion

    #region HUDManager Events
    public class BuildEntityEvent : Event
    {
        public Entity e;       
    }
    public class SpeedChangedEvent : Event
    {
        public int speed;      
    }
    public class DayEndedEvent : Event
    {
    }
    public class NightEndedEvent : Event
    {
    }
    public class FinalChallengeEvent : Event
    {
    }
    public class UpdateMinimapEvent : Event
    {
        public bool value;
    }
    #endregion

    #region BuildingHUD Events
    public class AllBuildingsBuiltEvent : Event
    {
    }
    #endregion

    #region MenuManager Events
    public class PlayButtonClickedEvent : Event
    {
        public bool newSave;
        public bool unpause;
    }
    public class PauseButtonClickedEvent : Event
    {
    }
    public class MenuButtonClickedEvent : Event
    {
    }
    #endregion

    #region PreferenceManager Events
    public class LoadDataEvent : Event
    {
    }
    public class ApplyDataEvent : Event
    {
    }
    public class SaveDataEvent : Event
    {
        public GAMESTATE currentPlayState;
        public float light;
        public int day;
        public bool finalChallenge;
    }
    public class EraseDataEvent : Event
    {
    }
    public class DataLoadedEvent : Event
    {
        public SavedData save;
        public SettingsData settings;
    }
    public class DataSavedEvent : Event
    {
    }
    public class DataAppliedEvent : Event
    {

    }
    public class DataErasedEvent : Event
    {
    }
    public class CameraMovedEvent : Event
    {
        public Vector3 position;
        public Quaternion rotation;
        public float zoomLevel;
    }
    #endregion

    #region BeaconSystem Events
    public class SubSceneLoadedEvent : Event
    {
    }
    public class LightLevelChangedEvent : Event
    {
        public float lightLevel;
    }
    #endregion

}