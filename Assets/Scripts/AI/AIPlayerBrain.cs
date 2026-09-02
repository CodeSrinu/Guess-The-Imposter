using System.Collections.Generic;
using UnityEngine;

public class AIPlayerBrain 
{
    public string aiName { get; }
    public string id {  get; }
    public bool isImposter = false;

    //personality
    [Range(0f, 1f)] public float talktivness = 0;
    [Range(0f, 1f)] public float aggression = 0;
    [Range(0f, 1f)] public float suspicion_bias = 0;
    [Range(0f, 1f)] public float trust_bias = 0;
    [Range(0f, 1f)] public float bluff_tendency = 0;
    [Range(0f, 1f)] public float defensiveness = 0;

    //belief-state
    public Dictionary<string, float> suspicionMap = new Dictionary<string, float>();
    public Dictionary<string, float> trustMap = new Dictionary<string, float>();
    public string infferedCivilianWord;
    [Range(0f,1f)] public float infferedWordConfidence;

    public AIPlayerBrain(string aiName, string id)
    {
        this.aiName = aiName;
        this.id = id;
    }

    public AIPlayerBrain Initialize(List<string> allPlayerIds)
    {
        foreach(string id in allPlayerIds)
        {
            if(id != this.id)
            {
                trustMap[id] = 0.5f;
                suspicionMap[id] = 0.5f;
            }
        }
        return this;
    }

    public enum CharacterArchetype
    {
        AnalyticalQuiet,
        Manipulative,
        Chaotic,
        Aggressive
    }

    [System.Serializable]
    public struct MemoryEntry
    {
        public int round;
        public string player;
        public string eventType;
        public string details;
        public string aiReaction;
    }

    [System.Serializable]
    public struct CharacterProfile
    {
        public CharacterArchetype archetype;
        public float talkativeness;
        public float aggression;
        public float suspicionBias;
        public float trustBias;
        public float bluffTendency;
        public float defensiveness;
        public string description;


    }
    public static CharacterProfile AnalyticalQuiet => new CharacterProfile
    {
        archetype = CharacterArchetype.AnalyticalQuiet,
        talkativeness = 0.3f,
        aggression = 0.2f,
        suspicionBias = 0.6f,
        trustBias = 0.5f,
        bluffTendency = 0.2f,
        defensiveness = 0.6f,
        description = "You are quiet and analytical. You observe carefully before speaking. When you do talk, you reference specific clues. You speak in short, measured sentences."
    };

    public static CharacterProfile Manipulative => new CharacterProfile
    {
        archetype = CharacterArchetype.Manipulative,
        talkativeness = 0.7f,
        aggression = 0.4f,
        suspicionBias = 0.7f,
        trustBias = 0.2f,
        bluffTendency = 0.85f,
        defensiveness = 0.4f,
        description = "You are confident and strategic. You steer conversations toward others before they point at you. You use flattery and deflection. You never seem nervous."
    };

    public static CharacterProfile Chaotic => new CharacterProfile
    {
        archetype = CharacterArchetype.Chaotic,
        talkativeness = 0.75f,
        aggression = 0.8f,
        suspicionBias = 0.4f,
        trustBias = 0.3f,
        bluffTendency = 0.7f,
        defensiveness = 0.2f,
        description = "You are unpredictable and impulsive. You speak before thinking. You change topics suddenly. You accuse people for no clear reason sometimes."
    };

    public static CharacterProfile Aggressive => new CharacterProfile
    {
        archetype = CharacterArchetype.Aggressive,
        talkativeness = 0.65f,
        aggression = 0.85f,
        suspicionBias = 0.7f,
        trustBias = 0.25f,
        bluffTendency = 0.25f,
        defensiveness = 0.5f,
        description = "You are direct and confrontational. You say exactly what you think. You don't apologize or soften your accusations. You push back hard when challenged."
    };

    public bool ShouldSpeakThisRound(int round, bool wasAccusedLastRound, CharacterProfile profile)
    {
        float speakProbability;
        
        speakProbability = profile.talkativeness;

        if (round == 1) speakProbability *= 0.7f;
        if(wasAccusedLastRound) speakProbability += 0.3f;
        switch (profile.archetype)
        {
            case CharacterArchetype.Chaotic: 
                speakProbability += 0.1f;
                break;
            case CharacterArchetype.Aggressive:
                speakProbability += 0.2f;
                break;
        }

        speakProbability = Mathf.Clamp01(speakProbability);

        return speakProbability > Random.Range(0, 1);
    }

    public Player
}
