using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using UnityEngine.UI;
using TMPro;
using Kryz.Tweening;
using System.Collections;
using Unity.Transforms;

namespace Beacon
{
    public class BuildingHUDManager : MonoBehaviour
    {
        private Entity selectedBuilding;
        private Entity beacon;
        private EntityQuery clickedBuildingQuery;
        private EntityQuery beaconQuery;
        private EntityManager _entityManager;

        private Building building;
        private Health buildingHealth;
        private Beacon beaconHealth;

        [Header("Root HUD Element")]
        [SerializeField] private GameObject BuildingHUD;

        [Header("HUD Elements")]
        [SerializeField] private GameObject BuildingName;
        [SerializeField] private GameObject BuildingLife;
        [SerializeField] private GameObject BuildingLifeTxt;

        [Header("Button Elements")]
        [SerializeField] private GameObject BuyingButton;
        [Space(5)]
        [SerializeField] private GameObject AttackModeButton;
        [SerializeField] private GameObject AttackUpgradeButton;
        [SerializeField] private GameObject AttackMaxLevel;
        [SerializeField] private GameObject AttackTicks;
        [Space(5)]
        [SerializeField] private GameObject DefenseModeButton;
        [SerializeField] private GameObject DefenseUpgradeButton;
        [SerializeField] private GameObject DefenseMaxLevel;
        [SerializeField] private GameObject DefenseTicks;
        [Space(5)]
        [SerializeField] private GameObject ProductionModeButton;
        [SerializeField] private GameObject ProductionUpgradeButton;
        [SerializeField] private GameObject ProductionMaxLevel;
        [SerializeField] private GameObject ProductionTicks;

        [Header("Tooltip Elements")]
        [SerializeField] private GameObject BuyingButtonTooltip;
        [SerializeField] private GameObject BuyingButtonTooltipText;
        [Space(5)]
        [SerializeField] private GameObject AttackModeButtonTooltip;
        [SerializeField] private GameObject AttackUpgradeButtonTooltip;
        [SerializeField] private GameObject AttackUpgradeButtonTooltipText;
        [SerializeField] private GameObject AttackUpgradeButtonTooltipPrice;
        [Space(5)]
        [SerializeField] private GameObject DefenseModeButtonTooltip;
        [SerializeField] private GameObject DefenseUpgradeButtonTooltip;
        [SerializeField] private GameObject DefenseUpgradeButtonTooltipText;
        [SerializeField] private GameObject DefenseUpgradeButtonTooltipPrice;
        [Space(5)]
        [SerializeField] private GameObject ProductionModeButtonTooltip;
        [SerializeField] private GameObject ProductionUpgradeButtonTooltip;
        [SerializeField] private GameObject ProductionUpgradeButtonTooltipText;
        [SerializeField] private GameObject ProductionUpgradeButtonTooltipPrice;

        [Header("Outline System")]
        [SerializeField] private GameObject OutlinePrefab;
        [SerializeField] private Material MaterialDestroyed;
        [SerializeField] private Material MaterialBuilt;
        private GameObject Outline;


        private TextMeshProUGUI BuildingNameText;
        private RectTransform BuildingLifeTransform;
        private TextMeshProUGUI BuildingLifeText;

        private RectTransform panel;

        private Image BuyingImage;
        private Image AttackModeImage;
        private Image DefenseModeImage;
        private Image ProductionModeImage;

        private Image AttackUpgradeImage;
        private Image DefenseUpgradeImage;
        private Image ProductionUpgradeImage;

        private Color32 Active = new Color32(255, 255, 255, 255);
        private Color32 Inactive = new Color32(150, 150, 150, 150);

        private RectTransform AttackTicksTransform;
        private RectTransform DefenseTicksTransform;
        private RectTransform ProductionTicksTransform;

        private TextMeshProUGUI BuyingButtonTooltipTextMesh;

        private TextMeshProUGUI AttackUpgradeButtonTooltipTextMesh;
        private TextMeshProUGUI AttackUpgradeButtonTooltipPriceMesh;
        private TextMeshProUGUI DefenseUpgradeButtonTooltipTextMesh;
        private TextMeshProUGUI DefenseUpgradeButtonTooltipPriceMesh;
        private TextMeshProUGUI ProductionUpgradeButtonTooltipTextMesh;
        private TextMeshProUGUI ProductionUpgradeButtonTooltipPriceMesh;

        private bool FinalHUDShown = false;

        public static BuildingHUDManager instance;
        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(gameObject);
        }

        private void OnEnable()
        {
            EventManager.Instance.AddListener<GamePlayNightEvent>(onGamePlayNightEvent);
        }

        private void OnDisable()
        {
            EventManager.Instance.RemoveListener<GamePlayNightEvent>(onGamePlayNightEvent);
        }

        private void Start()
        {
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            clickedBuildingQuery = _entityManager.CreateEntityQuery(new ComponentType[] { typeof(Clicked), typeof(Building) });
            beaconQuery = _entityManager.CreateEntityQuery(new ComponentType[] { typeof(Beacon) });

            panel = BuildingHUD.transform.GetChild(0).gameObject.GetComponent<RectTransform>();

            BuildingHUD.SetActive(true);

            BuildingNameText = BuildingName.GetComponent<TextMeshProUGUI>();
            BuildingLifeTransform = BuildingLife.GetComponent<RectTransform>();
            BuildingLifeText = BuildingLifeTxt.GetComponent<TextMeshProUGUI>();

            BuyingImage = BuyingButton.GetComponent<Image>();
            AttackModeImage = AttackModeButton.GetComponent<Image>();
            DefenseModeImage = DefenseModeButton.GetComponent<Image>();
            ProductionModeImage = ProductionModeButton.GetComponent<Image>();

            AttackUpgradeImage = AttackUpgradeButton.GetComponent<Image>();
            DefenseUpgradeImage = DefenseUpgradeButton.GetComponent<Image>();
            ProductionUpgradeImage = ProductionUpgradeButton.GetComponent<Image>();

            AttackTicksTransform = AttackTicks.GetComponent<RectTransform>();
            DefenseTicksTransform = DefenseTicks.GetComponent<RectTransform>();
            ProductionTicksTransform = ProductionTicks.GetComponent<RectTransform>();

            BuyingButtonTooltipTextMesh = BuyingButtonTooltipText.GetComponent<TextMeshProUGUI>();

            AttackUpgradeButtonTooltipTextMesh = AttackUpgradeButtonTooltipText.GetComponent<TextMeshProUGUI>();
            AttackUpgradeButtonTooltipPriceMesh = AttackUpgradeButtonTooltipPrice.GetComponent<TextMeshProUGUI>();
            DefenseUpgradeButtonTooltipTextMesh = DefenseUpgradeButtonTooltipText.GetComponent<TextMeshProUGUI>();
            DefenseUpgradeButtonTooltipPriceMesh = DefenseUpgradeButtonTooltipPrice.GetComponent<TextMeshProUGUI>();
            ProductionUpgradeButtonTooltipTextMesh = ProductionUpgradeButtonTooltipText.GetComponent<TextMeshProUGUI>();
            ProductionUpgradeButtonTooltipPriceMesh = ProductionUpgradeButtonTooltipPrice.GetComponent<TextMeshProUGUI>();
        }

        private void FixedUpdate()
        {
            if (clickedBuildingQuery.IsEmpty)
            {
                if (BuildingHUD.activeSelf)
                {
                    DestroyOutline();
                    StartCoroutine(Coroutines.MyMenuCloseCouroutine(BuildingHUD, panel, new Vector2(0, 0), new Vector2(0, -100), 2, EasingFunctions.OutCubic));
                }
                return;
            }

            if (!BuildingHUD.activeSelf)
            {
                if (Outline == null)
                    BuildOutline();
                StartCoroutine(Coroutines.MyMenuOpenCouroutine(BuildingHUD, panel, new Vector2(0, -100), new Vector2(0, 0), 2, EasingFunctions.OutCubic));
            }

            foreach(var entity in clickedBuildingQuery.ToEntityArray(Allocator.Temp))
                selectedBuilding = entity;

            foreach (var entity in beaconQuery.ToEntityArray(Allocator.Temp))
                beacon = entity;

            building = _entityManager.GetComponentData<Building>(selectedBuilding);
            buildingHealth = _entityManager.GetComponentData<Health>(selectedBuilding);
            beaconHealth = _entityManager.GetComponentData<Beacon>(beacon);

            MoveOutline();

            bool destroyed = building.isDestroyed;
            string ID = building.ID.ToString();

            float health = destroyed ? 0.0f : buildingHealth.health;
            float maxhealth = buildingHealth.maxHealth;

            float life = health * 165.0f / maxhealth;
            life = destroyed ? 0.0f : Mathf.Clamp(Round(life, 2), 0.0f, 165.0f);

            bool needRepair = health < maxhealth;
            bool canRepair = RepairPrice() < beaconHealth.lightLevel;

            if (GameManager.instance.isDay)
            {
                BuyingImage.color = !needRepair ? Inactive : canRepair ? Active : Inactive;
                AttackModeImage.color = (destroyed ? Inactive : building.mode == Mode.attack ? Active : Inactive);
                DefenseModeImage.color = (destroyed ? Inactive : building.mode == Mode.defense ? Active : Inactive);
                ProductionModeImage.color = (destroyed ? Inactive : building.mode == Mode.production ? Active : Inactive);
            }
            else
            {
                BuyingImage.color = Inactive;
                AttackModeImage.color = (destroyed ? Inactive : building.mode == Mode.attack ? Active : Inactive);
                DefenseModeImage.color = (destroyed ? Inactive : building.mode == Mode.defense ? Active : Inactive);
                ProductionModeImage.color = (destroyed ? Inactive : building.mode == Mode.production ? Active : Inactive);
            }

            BuildingNameText.text = "Memory #" + ID;
            BuildingLifeTransform.sizeDelta = new Vector2(life, 16);
            BuildingLifeText.text = (Round(health, 2)).ToString() + " / " + maxhealth.ToString();

            BuildUpgrader(AttackUpgradeButton, AttackMaxLevel, AttackUpgradeImage, AttackTicksTransform, building.lvlAttack);
            BuildUpgrader(DefenseUpgradeButton, DefenseMaxLevel, DefenseUpgradeImage, DefenseTicksTransform, building.lvlDefense);
            BuildUpgrader(ProductionUpgradeButton, ProductionMaxLevel, ProductionUpgradeImage, ProductionTicksTransform, building.lvlProduction);
        }

        private float RepairPrice()
        {
            if (buildingHealth.maxHealth - buildingHealth.health == 0)
            {
                BuyingButtonTooltipTextMesh.text = "No need to repair";
                return 0;
            }

            float repairPrice = 0;

            if (building.isDestroyed)
                repairPrice += building.buildingPrice;

            if (buildingHealth.health > 10)
                repairPrice += (buildingHealth.maxHealth - buildingHealth.health) / 2;

            BuyingButtonTooltipTextMesh.text = "Price " + Round(repairPrice, 1);
            return repairPrice;
        }

        private void BuildOutline()
        {
            Outline = Instantiate(OutlinePrefab);
        }

        private void MoveOutline()
        {
            if (Outline == null)
                return;

            LocalTransform buildingPos = _entityManager.GetComponentData<LocalTransform>(selectedBuilding);
            Outline.transform.position = buildingPos.Position;
            if (building.isDestroyed)
            {
                Outline.transform.localScale = new Vector3(50, 50, 50);
                Outline.GetComponent<MeshRenderer>().material = MaterialDestroyed;

            }
            else
            {
                Outline.transform.localScale = new Vector3(51, 51, 51);
                Outline.GetComponent<MeshRenderer>().material = MaterialBuilt;
            }
        }

        private void DestroyOutline()
        {
            Destroy(Outline);
            Outline = null;
        }

        private void BuildUpgrader(GameObject button, GameObject MaxLevel, Image buttonImg, RectTransform ticks, float level)
        {
            if (level >= 11)
            {
                button.SetActive(false);
                MaxLevel.SetActive(true);
            }
            else
            {
                button.SetActive(true);
                MaxLevel.SetActive(false);
                if (beaconHealth.lightLevel - (level+1) <= 0 || GameManager.instance.isNight)
                {
                    buttonImg.color = Inactive;
                }
                else
                {
                    buttonImg.color = Active;
                }
            }
            ticks.sizeDelta = new Vector2(Mathf.Clamp(30 * level, 0, 330), 11);
        }

        public static float Round(float value, int digits)
        {
            float mult = Mathf.Pow(10.0f, (float)digits);
            return Mathf.Round(value * mult) / mult;
        }

        #region Button Callbacks
        public void BuyBuilding()
        {
            if (GameManager.instance.isNight)
                return;

            float repairPrice = RepairPrice();
            if (repairPrice < beaconHealth.lightLevel)
            {
                if (building.isDestroyed)
                {
                    _entityManager.SetComponentEnabled<InConstruction>(selectedBuilding, true);
                    StartCoroutine(MyFunctionCheckHousesWithDelay());
                    
                }
                else
                {
                    beaconHealth.lightLevel -= repairPrice;
                    _entityManager.SetComponentData<Beacon>(beacon, beaconHealth);

                    buildingHealth.health = buildingHealth.maxHealth;
                    _entityManager.SetComponentData<Health>(selectedBuilding, buildingHealth);
                }
            }
        }

        IEnumerator MyFunctionCheckHousesWithDelay()
        {
            yield return new WaitForSecondsRealtime(1f);
            if (GetAllHouseBuilt())
                EventManager.Instance.Raise(new AllBuildingsBuiltEvent() { });
        }

        private bool GetAllHouseBuilt()
        {
            if (FinalHUDShown)
                return false;

            int nbNotBuilt = 0;
            
            var builder = new EntityQueryBuilder(Allocator.Temp);
            builder.WithAll<Building, Health>();
            EntityQuery buildingQuery = _entityManager.CreateEntityQuery(builder);
            NativeArray<Entity> entities = buildingQuery.ToEntityArray(Allocator.TempJob);

            if (entities.Length != 0)
            {
                for (int i = 0; i < entities.Length; i++)
                {
                    Health building = _entityManager.GetComponentData<Health>(entities[i]);
                    if (building.health == 0)
                        nbNotBuilt++;
                }
                Tools.LOG(this, nbNotBuilt + " / " + entities.Length + " houses are not built");
                if (nbNotBuilt == 0)
                {
                    FinalHUDShown = true;
                    return true;
                }
                return false;
            }
            else
            {
               return false;
            }
        }

        public void SetMode(int mode)
        {
            if (GameManager.instance.isNight)
                return;

            building = _entityManager.GetComponentData<Building>(selectedBuilding);
            buildingHealth = _entityManager.GetComponentData<Health>(selectedBuilding);
            beaconHealth = _entityManager.GetComponentData<Beacon>(beacon);
            Mode oldMode = building.mode;
            switch (mode)
            {
                case 0:
                    building.mode = Mode.attack;
                    break;
                case 1:
                    building.mode = Mode.defense;
                    break;
                case 2:
                    building.mode = Mode.production;
                    break;
                default:
                    building.mode = Mode.attack;
                    break;
            }
            if(oldMode != building.mode)
            {
                _entityManager.SetComponentData<Building>(selectedBuilding, building);
                _entityManager.SetComponentEnabled<ChangeModeTag>(selectedBuilding, true);
            }
            
        }

        public void UpgradeAttack()
        {
            if (GameManager.instance.isNight)
                return;

            if (beaconHealth.lightLevel - (building.lvlAttack+1) > 0)
            {
                //Tools.LOG(this, "Enought light to update Att (" + (building.lvlAttack + 1) + " / " + beaconHealth.lightLevel + ")");

                beaconHealth.lightLevel = beaconHealth.lightLevel - (building.lvlAttack+1);
                _entityManager.SetComponentData<Beacon>(beacon, beaconHealth);

                building.nbSoldierMAX = building.nbSoldierMAX + 1;
                building.lvlAttack = building.lvlAttack + 1;
                _entityManager.SetComponentData<Building>(selectedBuilding, building);
                AttackUpgradeButtonTooltipTextMesh.text = "Increase soldiers count\nCurrent " + building.nbSoldierMAX + ", Next " + (building.nbSoldierMAX + 1) + "";
                AttackUpgradeButtonTooltipPriceMesh.text = "Price: " + (building.lvlAttack + 1) + "";

                if (building.lvlAttack == 11)
                    AttackUpgradeButtonTooltip.SetActive(false);
            }
            else
            {
                //Tools.LOG(this, "Not enought light to update Att (" + (building.lvlAttack + 1) + " / " + beaconHealth.lightLevel + ")");
            }
        }
        public void UpgradeDefense()
        {
            if (GameManager.instance.isNight)
                return;

            if (beaconHealth.lightLevel - (building.lvlDefense+1) > 0)
            {
                //Tools.LOG(this, "Enought light to update Def (" + (building.lvlDefense + 1) + " / " + beaconHealth.lightLevel + ")");

                beaconHealth.lightLevel = beaconHealth.lightLevel - (building.lvlDefense+1);
                _entityManager.SetComponentData<Beacon>(beacon, beaconHealth);

                buildingHealth.maxHealth += 10;
                buildingHealth.health += 10;
                building.lvlDefense = building.lvlDefense + 1;
                _entityManager.SetComponentData<Building>(selectedBuilding, building);
                _entityManager.SetComponentData<Health>(selectedBuilding, buildingHealth);
                DefenseUpgradeButtonTooltipTextMesh.text = "Increase building life\nCurrent " + buildingHealth.maxHealth + ", Next " + (buildingHealth.maxHealth + 10) + "";
                DefenseUpgradeButtonTooltipPriceMesh.text = "Price: " + (building.lvlDefense + 1) + "";

                if (building.lvlDefense == 11)
                    DefenseUpgradeButtonTooltip.SetActive(false);
            }
            else
            {
                //Tools.LOG(this, "Not enought light to update Def (" + (building.lvlDefense + 1) + " / " + beaconHealth.lightLevel + ")");
            }
        }
        public void UpgradeProduction()
        {
            if (GameManager.instance.isNight)
                return;

            if (beaconHealth.lightLevel - (building.lvlProduction+1) > 0)
            {
                //Tools.LOG(this, "Enought light to update Prod (" + (building.lvlProduction + 1) + " / " + beaconHealth.lightLevel + ")");

                beaconHealth.lightLevel = beaconHealth.lightLevel - (building.lvlProduction+1);
                _entityManager.SetComponentData<Beacon>(beacon, beaconHealth);

                building.lvlProduction = building.lvlProduction + 1;
                building.production = building.lvlProduction;
                _entityManager.SetComponentData<Building>(selectedBuilding, building);
                ProductionUpgradeButtonTooltipTextMesh.text = "Increase light production\nCurrent " + building.production + "%, Next " + (building.production + 1) + "%";
                ProductionUpgradeButtonTooltipPriceMesh.text = "Price: " + (building.lvlProduction + 1) + "";


                if (building.lvlProduction == 11)
                    ProductionUpgradeButtonTooltip.SetActive(false);
            }
            else
            {
                //Tools.LOG(this, "Not enought light to update Prod (" + (building.lvlProduction + 1) + " / " + beaconHealth.lightLevel + ")");
            }
        }

        public void onButtonRepairUpgradeMouseOver(bool enter)
        {
            BuyingButtonTooltip.SetActive(enter);
        }
        public void onButtonAttackModeMouseOver(bool enter)
        {
            AttackModeButtonTooltip.SetActive(enter);
        }
        public void onButtonAttackUpgradeMouseOver(bool enter)
        {
            AttackUpgradeButtonTooltipTextMesh.text = "Increase soldiers count\nCurrent " + building.nbSoldierMAX + ", Next " + (building.nbSoldierMAX + 1) + "";
            if (GameManager.instance.isDay)
                AttackUpgradeButtonTooltipPriceMesh.text = "Price: " + (building.lvlAttack + 1) + "";
            else
                AttackUpgradeButtonTooltipPriceMesh.text = "Wait day to upgrade";
            AttackUpgradeButtonTooltip.SetActive(enter);
        }
        public void onButtonDefenseModeMouseOver(bool enter)
        {
            DefenseModeButtonTooltip.SetActive(enter);
        }
        public void onButtonDefenseUpgradeMouseOver(bool enter)
        {
            DefenseUpgradeButtonTooltipTextMesh.text = "Increase building life\nCurrent " + buildingHealth.maxHealth + ", Next " + (buildingHealth.maxHealth + 10) + "";
            if (GameManager.instance.isDay)
                DefenseUpgradeButtonTooltipPriceMesh.text = "Price: " + (building.lvlDefense + 1) + "";
            else
                DefenseUpgradeButtonTooltipPriceMesh.text = "Wait day to upgrade";
            DefenseUpgradeButtonTooltip.SetActive(enter);
        }
        public void onButtonProductionModeMouseOver(bool enter)
        {
            ProductionModeButtonTooltip.SetActive(enter);
        }
        public void onButtonProductionUpgradeMouseOver(bool enter)
        {
            ProductionUpgradeButtonTooltipTextMesh.text = "Increase light production\nCurrent " + building.production + "%, Next " + (building.production + 1) + "%";
            if (GameManager.instance.isDay)
                ProductionUpgradeButtonTooltipPriceMesh.text = "Price: " + (building.lvlProduction + 1) + "";
            else
                ProductionUpgradeButtonTooltipPriceMesh.text = "Wait day to upgrade";
            ProductionUpgradeButtonTooltip.SetActive(enter);
        }
        #endregion

        #region Event Callbacks
        public void onGamePlayNightEvent(GamePlayNightEvent e)
        {
        }
        #endregion
    }
}
