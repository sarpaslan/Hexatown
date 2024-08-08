using UnityEngine;

public abstract class ProductionPlace : WorkPlace
{
    [UsefullInfo(readOnly: true)]
    public bool CanPlaceWork => CanWork();
    public abstract UnityEngine.Sprite RequiresIcon { get; }
    public abstract Stats Requires { get; }
    public abstract Stats Produce { get; }

    [UsefullInfo(readOnly: true)]
    public float WorkPercent;

    [UsefullInfo(isCheatToChange: true)]
    public int RequireAmount = 0;


    private int m_produceAmount;
    [UsefullInfo(isCheatToChange: true)]
    public virtual int ProduceAmount
    {
        get
        {
            return m_produceAmount + (int)(m_produceAmount * BonusProduction / 100f);
        }
        set
        {
            m_produceAmount = value;
        }
    }

    [UsefullInfo(readOnly: true)]
    public bool IsInventoryFull = false;

    [UsefullInfo(readOnly: true)]
    public bool ReadyForWork = false;

    public override bool CanWork()
    {
        return base.CanWork() && IsInventoryFull;
    }
    public ProductionPlace(Card card, Vector3Int pos, int workerCount
    , int requireAmount, int produceAmount) : base(card, pos, workerCount)
    {
        this.RequireAmount = requireAmount;
        this.ProduceAmount = produceAmount;
    }
    public override void OnPlaced(bool silent)
    {
        base.OnPlaced(silent);
        this.ProduceAmount += (int)(ProduceAmount * BonusProduction / 100f);
    }
    public virtual void OnRequest()
    {
        if (IsInventoryFull) return;

        if (this.Requires == Stats.NONE)
        {
            IsInventoryFull = true;
        }
        else
        {
            if (GameController.Stats.Spend(this.Requires, RequireAmount))
            {
                IsInventoryFull = true;
            }
            else
            {
                IsInventoryFull = false;
            }
        }


        if (!IsInventoryFull)
            if (Error == PlaceError.NONE)
                Error = PlaceError.NOT_ENOUGH_RESOURCES;

        if (IsInventoryFull)
            if (Error == PlaceError.NOT_ENOUGH_RESOURCES)
                Error = PlaceError.NONE;
    }
    public override void OnTick()
    {
        base.OnTick();
        if (!IsInventoryFull)
        {
            BeforeBegin();
        }
        if (CanWork() && ReadyForWork)
        {
            WorkPercent += 1;
            if (WorkPercent >= 100)
            {
                IsInventoryFull = false;
                ReadyForWork = false;
                OnCompletedProduction();
            }
            NotifyPropertyChange();
        }
    }

    private void BeforeBegin()
    {
        OnRequest();
    }

    public virtual void OnCompletedProduction()
    {
    }
}
