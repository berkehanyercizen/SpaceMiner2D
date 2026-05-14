namespace SpaceMining
{
    public enum ModifierType
    {
        SlotCount,
        DroneSpeed,
        AttackDuration,
        SpawnInterval
    }

    [System.Serializable]
    public class Modifier
    {
        public ModifierType type;
        public float magnitude;
        public string label;
        public bool isBuff;
    }
}
