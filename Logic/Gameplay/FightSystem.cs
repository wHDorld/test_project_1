using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gameplay;

public class FightSystem : MonoBehaviour
{
    public bool InitiateFightFromStart;

    [Header("Units")]
    public Unit[] LeftUnitsData;
    public Unit[] RightUnitsData;
    [Header("Tactical Information")]
    public float DistanceBetweenUnits = 5;

    TurnState current_state;
    int current_unit_turn = 0;
    int current_unit_select = 0;
    bool is_mouse_over_enemy = false;
    List<UnitObject> leftUnitsObject = new List<UnitObject>();
    List<UnitObject> rightUnitsObject = new List<UnitObject>();
    List<UnitObject> wholeUnitsObject = new List<UnitObject>();

    void Start()
    {
        if (InitiateFightFromStart) InitiateFight();
        current_state = TurnState.PlayerChooseOption;
    }

    void Update()
    {
        PlayerMouseControll();
        UpdateSelectControll();
        UpdateCinematicActionEffect();
    }

    #region Initiate Fight
    public void InitiateFight()
    {
        InitiateUnits();
        InitiateEvents();
        StartCoroutine(TurnJob());
    }

    void InitiateUnits()
    {
        for (int i = 0; i < LeftUnitsData.Length; i++)
            leftUnitsObject.Add(new UnitObject(LeftUnitsData[i], gameObject, this, true, new Vector3(-(i + 1) * DistanceBetweenUnits, 0, 0)));
        for (int i = 0; i < RightUnitsData.Length; i++)
            rightUnitsObject.Add(new UnitObject(RightUnitsData[i], gameObject, this, false, new Vector3((i + 1) * DistanceBetweenUnits, 0, 0)));

        wholeUnitsObject.AddRange(leftUnitsObject);
        wholeUnitsObject.AddRange(rightUnitsObject);
    }
    void InitiateEvents()
    {
        foreach (var a in wholeUnitsObject)
        {
            a.Animator.UnitAnimationSkeleton.state.Complete += AnimationCompleteListener;
            a.Animator.UnitAnimationSkeleton.state.Event += AnimationEventsListener;
        }
    }
    #endregion

    #region UpdateControll
    void PlayerInputControll()
    {
        if (is_mouse_over_enemy && Input.GetMouseButton(0))
            current_state = TurnState.PlayerToEnemyAction;
    }
    void PlayerMouseControll()
    {
        if (current_state != TurnState.PlayerChooseTarget) return;

        is_mouse_over_enemy = false;
        int pos = Mathf.RoundToInt(transform.InverseTransformPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition)).x / DistanceBetweenUnits) - 1;
        if (pos < 0 || pos >= rightUnitsObject.Count) return;
        is_mouse_over_enemy = true;
        current_unit_select = pos;
    }
    void UpdateSelectControll()
    {
        switch (current_state)
        {
            case TurnState.PlayerChooseOption:
                for (int i = 0; i < leftUnitsObject.Count; i++)
                    if (i == current_unit_turn) leftUnitsObject[i].UI.TurnSelect();
                    else leftUnitsObject[i].UI.Deselect();
                foreach (var a in rightUnitsObject)
                    a.UI.NeutralDeselect();
                break;
            case TurnState.PlayerChooseTarget:
                for (int i = 0; i < leftUnitsObject.Count; i++)
                    if (i == current_unit_turn) leftUnitsObject[i].UI.TurnSelect();
                    else leftUnitsObject[i].UI.NeutralDeselect();
                for (int i = 0; i < rightUnitsObject.Count; i++)
                    if (i == current_unit_select) rightUnitsObject[i].UI.MouseSelect();
                    else rightUnitsObject[i].UI.Deselect();
                break;

            case TurnState.EnemyChooseOption:
                for (int i = 0; i < rightUnitsObject.Count; i++)
                    if (i == current_unit_turn) rightUnitsObject[i].UI.TurnSelect();
                    else rightUnitsObject[i].UI.Deselect();
                foreach (var a in leftUnitsObject)
                    a.UI.NeutralDeselect();
                break;
            case TurnState.EnemyChooseTarget:
                for (int i = 0; i < rightUnitsObject.Count; i++)
                    if (i == current_unit_turn) rightUnitsObject[i].UI.TurnSelect();
                    else rightUnitsObject[i].UI.NeutralDeselect();
                for (int i = 0; i < rightUnitsObject.Count; i++)
                    if (i == current_unit_select) leftUnitsObject[i].UI.MouseSelect();
                    else leftUnitsObject[i].UI.Deselect();
                break;

            default:
                foreach (var a in leftUnitsObject)
                    a.UI.NeutralDeselect();
                foreach (var a in rightUnitsObject)
                    a.UI.NeutralDeselect();
                break;
        }
    }
    void UpdateCinematicActionEffect()
    {
        if (isActionPhase)
        {
            GetCurrentAttackUnit.Cinematic.CinematicIncrease();
            GetCurrentHurtUnit.Cinematic.CinematicIncrease();
        }
        else
            foreach(var a in wholeUnitsObject)
                    a.Cinematic.CinematicDecrease();
    }
    #endregion

    #region UI Events
    public void PressAttackButton()
    {
        if (current_state == TurnState.PlayerChooseOption)
            MakeNextStep();
    }
    public void PressSkipButton()
    {
        if (current_state == TurnState.PlayerChooseOption)
            current_state = TurnState.EnemyChooseOption;
    }
    #endregion
    #region Animation Events
    private void AnimationCompleteListener(Spine.TrackEntry trackEntry)
    {
        if (GetCurrentAttackUnit == null) return;
        if (GetCurrentAttackUnit.Animator.UnitAnimationSkeleton.AnimationName != trackEntry.Animation.Name) return;
        if (GetCurrentAttackUnit.Animator.CurrentActionType != ActionType.Attack) return;
        GetCurrentAttackUnit.Animator.SetIdleAnimation_Randomly();
        GetCurrentHurtUnit.Animator.SetIdleAnimation_Randomly();
        MakeNextStep();
    }
    private void AnimationEventsListener(Spine.TrackEntry trackEntry, Spine.Event e)
    {
        if (e.Data.Name == "Hit")
        {
            GetCurrentHurtUnit.Animator.SetHurtAnimation_Randomly();
        }
    }
    #endregion
    #region Shortcuts

    public UnitObject GetCurrentAttackUnit
    {
        get
        {
            switch (current_state)
            {
                case TurnState.PlayerToEnemyAction: return leftUnitsObject[current_unit_turn];
                case TurnState.EnemyToPlayerAction: return rightUnitsObject[current_unit_turn];
                default: return null;
            }
        }
    }
    public UnitObject GetCurrentHurtUnit
    {
        get
        {
            switch (current_state)
            {
                case TurnState.PlayerToEnemyAction: return rightUnitsObject[current_unit_select];
                case TurnState.EnemyToPlayerAction: return leftUnitsObject[current_unit_select];
                default: return null;
            }
        }
    }

    public void MakeNextStep()
    {
        switch (current_state)
        {
            case TurnState.EnemyToPlayerAction: current_state = TurnState.PlayerChooseOption; break;
            default: current_state += 1; break;
        }
    }
    public void MakeAttack()
    {
        GetCurrentAttackUnit.Animator.SetAttackAnimation_Randomly();
    }

    public bool isActionPhase
    {
        get { return current_state == TurnState.PlayerToEnemyAction || current_state == TurnState.EnemyToPlayerAction; }
    }
    #endregion

    IEnumerator TurnJob()
    {
        while (true)
        {
            switch (current_state)
            {
                case TurnState.PlayerChooseOption:
                    yield return new WaitForEndOfFrame();
                    break;
                case TurnState.PlayerChooseTarget:
                    PlayerInputControll();
                    yield return new WaitForEndOfFrame();
                    break;
                case TurnState.PlayerToEnemyAction:
                    MakeAttack();
                    while (current_state == TurnState.PlayerToEnemyAction) yield return new WaitForEndOfFrame();
                    break;
                case TurnState.EnemyChooseOption:
                    current_unit_turn = Mathf.RoundToInt(Random.Range(0, rightUnitsObject.Count));
                    current_unit_select = Mathf.RoundToInt(Random.Range(0, leftUnitsObject.Count));
                    yield return new WaitForSeconds(1);
                    current_state = TurnState.EnemyChooseTarget;
                    break;
                case TurnState.EnemyChooseTarget:
                    yield return new WaitForSeconds(1);
                    current_state = TurnState.EnemyToPlayerAction;
                    break;
                case TurnState.EnemyToPlayerAction:
                    MakeAttack();
                    while (current_state == TurnState.EnemyToPlayerAction) yield return new WaitForEndOfFrame();
                    current_unit_turn = Mathf.RoundToInt(Random.Range(0, leftUnitsObject.Count));
                    break;
            }
        }
    }
}
