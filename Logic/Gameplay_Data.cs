using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using Spine;

namespace Gameplay
{
    #region UNIT
    public abstract class AUnit
    {
        UnitObject controller;

        public AUnit(UnitObject controller)
        {
            this.controller = controller;
        }

        public UnitObject Controller
        {
            set { }
            get { return controller; }
        }
    }

    public class UnitObject
    {
        public bool isLeft;

        MonoBehaviour monoBehaviour;
        GameObject unit_gameobject;

        Unit unit_data;
        UnitUI unit_ui;
        UnitAnimator unit_animator;
        UnitCinematic unit_cinematic;

        public UnitObject(Unit unit_data, GameObject parent, MonoBehaviour monoBehaviour, bool isLeft, Vector3 position)
        {
            this.isLeft = isLeft;
            this.monoBehaviour = monoBehaviour;
            this.unit_data = unit_data;

            InstantiateGameObjects();
            unit_gameobject.transform.localPosition = position;
            InstantiateSubSystems();

            unit_gameobject.transform.SetParent(parent.transform);
        }

        #region Instances
        void InstantiateGameObjects()
        {
            this.unit_gameobject = unit_data.InstantiateObject();
            if (!isLeft)
                unit_gameobject.transform.localScale = new Vector3(-1, 1, 1);
        }
        void InstantiateSubSystems()
        {
            unit_ui = new UnitUI(this);
            unit_animator = new UnitAnimator(this);
            unit_cinematic = new UnitCinematic(this);
        }
        #endregion        
        #region Properties

        public Unit Data
        {
            set { }
            get
            {
                return unit_data;
            }
        }
        public UnitUI UI
        {
            set { }
            get
            {
                return unit_ui;
            }
        }
        public UnitAnimator Animator
        {
            set { }
            get
            {
                return unit_animator;
            }
        }
        public UnitCinematic Cinematic
        {
            set { }
            get
            {
                return unit_cinematic;
            }
        }

        public MonoBehaviour GetMonoBehaviour
        {
            set { }
            get { return monoBehaviour; }
        }
        public GameObject UnitGameObject
        {
            set { }
            get
            {
                return unit_gameobject;
            }
        }
        #endregion
    }

    #region SubSystems
    public class UnitUI : AUnit
    {
        GameObject unit_ui_gameobject;
        Image selectImage;

        public UnitUI(UnitObject controller) : base(controller)
        {
            InitiateUI();
        }
        void InitiateUI()
        {
            unit_ui_gameobject = Controller.Data.InstantiateUIObject();
            Controller.GetMonoBehaviour.StartCoroutine(Update());
            selectImage = UnitUIGameObject.transform.Find("SelectImage").GetComponent<Image>();
        }

        #region Coroutines
        public IEnumerator Update()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();

                UnitUIGameObject.transform.position = Controller.UnitGameObject.transform.position;
            }
        }
        #endregion
        #region UI Methods
        public void MouseSelect()
        {
            selectImage.color = Controller.isLeft ? new Color(0, 1, 0, 1) : new Color(1, 0, 0, 1);
        }
        public void TurnSelect()
        {
            selectImage.color = Controller.isLeft ? new Color(0, 1, 0, 0.7f) : new Color(1, 0, 0, 0.7f);
        }
        public void Deselect()
        {
            selectImage.color = Controller.isLeft ? new Color(0, 1, 0, 0.3f) : new Color(1, 0, 0, 0.3f);
        }
        public void NeutralDeselect()
        {
            selectImage.color = new Color(1, 1, 1, 0.2f);
        }
        #endregion
        #region Properties
        public GameObject UnitUIGameObject
        {
            set { }
            get
            {
                return unit_ui_gameobject;
            }
        }
        #endregion
    }
    public class UnitAnimator : AUnit
    {
        SkeletonAnimation unit_skeleton;

        public UnitAnimator(UnitObject controller) : base(controller)
        {
            unit_skeleton = Controller.UnitGameObject.GetComponent<SkeletonAnimation>();

            unit_skeleton.state.Event += AnimationEventsListener;
            unit_skeleton.state.Start += AnimationStartListener;
            unit_skeleton.state.End += AnimationEndListener;

            SetIdleAnimation_Randomly();
        }

        #region Events
        private void AnimationEventsListener(TrackEntry trackEntry, Spine.Event e)
        {

        }
        private void AnimationStartListener(TrackEntry trackEntry)
        {

        }
        private void AnimationEndListener(TrackEntry trackEntry)
        {

        }
        #endregion
        #region Animation Methods
        public void SetIdleAnimation_Randomly()
        {
            unit_skeleton.state.SetAnimation(
                0,
                Controller.Data.A_Idle[Mathf.RoundToInt(Random.Range(0, Controller.Data.A_Idle.Length))],
                true);
            unit_skeleton.state.Update(Random.value);
        }
        public void SetAttackAnimation_Randomly()
        {
            unit_skeleton.state.SetAnimation(
                0,
                Controller.Data.A_Attack[Mathf.RoundToInt(Random.Range(0, Controller.Data.A_Attack.Length))],
                false);
        }
        public void SetHurtAnimation_Randomly()
        {
            unit_skeleton.state.SetAnimation(
                0,
                Controller.Data.A_Hurt[Mathf.RoundToInt(Random.Range(0, Controller.Data.A_Hurt.Length))],
                false);
        }
        #endregion
        #region Properties
        public SkeletonAnimation UnitAnimationSkeleton
        {
            set { }
            get
            {
                return unit_skeleton;
            }
        }
        public ActionType CurrentActionType
        {
            get
            {
                foreach (var a in Controller.Data.A_Idle)
                    if (unit_skeleton.AnimationName == a) return ActionType.Idle;
                foreach (var a in Controller.Data.A_Attack)
                    if (unit_skeleton.AnimationName == a) return ActionType.Attack;
                foreach (var a in Controller.Data.A_Hurt)
                    if (unit_skeleton.AnimationName == a) return ActionType.Hurt;
                return ActionType.Unknown;
            }
        }
        #endregion

    }
    public class UnitCinematic : AUnit
    {
        Vector3 original_scale;
        Vector3 original_pose;
        float scale_multiply = 2;

        public UnitCinematic (UnitObject controller) : base(controller)
        {
            original_scale = Controller.UnitGameObject.transform.localScale;
            original_pose = Controller.UnitGameObject.transform.localPosition;
        }

        public void CinematicIncrease()
        {
            Controller.UnitGameObject.transform.localScale = Vector3.Lerp(
                Controller.UnitGameObject.transform.localScale,
                original_scale * scale_multiply,
                15f * Time.deltaTime
                );

            Controller.UnitGameObject.transform.localPosition = Vector3.Lerp(
                Controller.UnitGameObject.transform.localPosition,
                new Vector3((Controller.isLeft ? -3 : 3) * scale_multiply, original_pose.y, original_pose.z),
                15f * Time.deltaTime
                );
        }
        public void CinematicDecrease()
        {
            Controller.UnitGameObject.transform.localScale = Vector3.Lerp(
                Controller.UnitGameObject.transform.localScale,
                original_scale,
                15f * Time.deltaTime
                );

            Controller.UnitGameObject.transform.localPosition = Vector3.Lerp(
                Controller.UnitGameObject.transform.localPosition,
                original_pose,
                15f * Time.deltaTime
                );
        }
    }
    #endregion
    #endregion
    #region TURN BASED
    public enum TurnState
    {
        PlayerChooseOption,
        PlayerChooseTarget,
        PlayerToEnemyAction,
        EnemyChooseOption,
        EnemyChooseTarget,
        EnemyToPlayerAction
    }
    public enum ActionType
    {
        Unknown = -1,
        Idle,
        Attack,
        Hurt
    }
    #endregion
}