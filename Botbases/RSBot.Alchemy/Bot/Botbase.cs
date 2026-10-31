using RSBot.Alchemy.Bundle;
using RSBot.Alchemy.Bundle.Attribute;
using RSBot.Alchemy.Bundle.Dismantle;
using RSBot.Alchemy.Bundle.Enhance;
using RSBot.Alchemy.Bundle.Magic;
using RSBot.Alchemy.Bundle.Manufacture;
using RSBot.Core;

namespace RSBot.Alchemy.Bot;

internal class Botbase
{
    #region Constructor

    public Botbase()
    {
        EnhanceBundle = new EnhanceBundle();
        MagicBundle = new MagicBundle();
        AttributeBundle = new AttributeBundle();
        ManufactureBundle = new ManufactureBundle();
        DismantleBundle = new DismantleBundle();
    }

    #endregion Constructor

    #region Properties

    /// <summary>
    ///     The selected botbase engine
    /// </summary>
    public AlchemyEngine AlchemyEngine { get; set; }

    /// <summary>
    ///     The enhancement manager used to increase item OptLevel
    /// </summary>
    public IAlchemyBundle EnhanceBundle { get; }

    /// <summary>
    ///     Gets or sets the attribute granter.
    /// </summary>
    /// <value>
    ///     The attribute granter.
    /// </value>
    public IAlchemyBundle AttributeBundle { get; set; }

    /// <summary>
    ///     The magic option manager used to fuse magic stones
    /// </summary>
    public IAlchemyBundle MagicBundle { get; set; }

    /// <summary>
    ///     The make-stuff manager used to resolve or compound alchemy materials
    /// </summary>
    public IAlchemyBundle ManufactureBundle { get; }

    /// <summary>
    ///     The dismantle manager used to break equipment into alchemy materials
    /// </summary>
    public IAlchemyBundle DismantleBundle { get; }

    /// <summary>
    ///     The configuration for the Enhancer
    /// </summary>
    public EnhanceBundleConfig EnhanceBundleConfig { get; set; }

    /// <summary>
    ///     The configuration for the magic option granter
    /// </summary>
    public MagicBundleConfig MagicBundleConfig { get; set; }

    /// <summary>
    ///     Gets or sets the attributes configuration.
    /// </summary>
    /// <value>
    ///     The attributes configuration.
    /// </value>
    public AttributeBundleConfig AttributeBundleConfig { get; set; }

    /// <summary>
    ///     Gets or sets the configuration for the manufacture manager.
    /// </summary>
    public ManufactureBundleConfig ManufactureBundleConfig { get; set; }

    /// <summary>
    ///     Gets or sets the configuration for the dismantle manager.
    /// </summary>
    public DismantleBundleConfig DismantleBundleConfig { get; set; }

    #endregion Properties

    #region Methods

    /// <summary>
    ///     Starts the botbase
    /// </summary>
    public void Start()
    {
        if (AlchemyEngine == AlchemyEngine.Enhance)
        {
            if (EnhanceBundleConfig == null)
            {
                Log.Warn("[LuckyAlchemyBot] Configuration issue detected!");

                Kernel.Bot.Stop();

                return;
            }

            EnhanceBundle?.Start();
        }

        if (AlchemyEngine == AlchemyEngine.Magic)
        {
            if (MagicBundleConfig == null)
            {
                Log.Warn("[LuckyAlchemyBot] Configuration issue detected!");

                Kernel.Bot.Stop();

                return;
            }

            MagicBundle?.Start();
        }

        if (AlchemyEngine == AlchemyEngine.Attribute)
        {
            if (AttributeBundleConfig == null)
            {
                Log.Warn("[LuckyAlchemyBot] Configuration issue detected!");

                Kernel.Bot.Stop();

                return;
            }

            AttributeBundle?.Start();
        }

        if (AlchemyEngine == AlchemyEngine.Manufacture)
        {
            if (ManufactureBundleConfig?.Materials == null || ManufactureBundleConfig.Materials.Count == 0)
            {
                Log.Warn("[Alchemy] No materials selected for manufacture!");

                Kernel.Bot.Stop();

                return;
            }

            ManufactureBundle?.Start();
        }

        if (AlchemyEngine == AlchemyEngine.Dismantle)
        {
            if (DismantleBundleConfig?.Equipment == null || DismantleBundleConfig.Equipment.Count == 0)
            {
                Log.Warn("[Alchemy] No equipment selected for dismantling!");

                Kernel.Bot.Stop();

                return;
            }

            DismantleBundle?.Start();
        }
    }

    /// <summary>
    ///     Stops the botbase and all managers
    /// </summary>
    public void Stop()
    {
        if (AlchemyEngine == AlchemyEngine.Enhance)
            EnhanceBundle?.Stop();

        if (AlchemyEngine == AlchemyEngine.Magic)
            MagicBundle?.Stop();

        if (AlchemyEngine == AlchemyEngine.Attribute)
            AttributeBundle?.Stop();

        if (AlchemyEngine == AlchemyEngine.Manufacture)
            ManufactureBundle?.Stop();

        if (AlchemyEngine == AlchemyEngine.Dismantle)
            DismantleBundle?.Stop();
    }

    /// <summary>
    ///     Sends the run command to the current manager
    /// </summary>
    public void Tick()
    {
        switch (AlchemyEngine)

        {
            case AlchemyEngine.Enhance:
                if (EnhanceBundleConfig == null || EnhanceBundle == null)
                {
                    Log.Warn("[Alchemy] Configuration issue detected!");

                    Kernel.Bot.Stop();

                    return;
                }

                EnhanceBundle.Run(EnhanceBundleConfig);

                break;

            case AlchemyEngine.Magic:
                if (MagicBundle == null || MagicBundleConfig == null)
                {
                    Log.Warn("[Alchemy] Configuration issue detected!");

                    Kernel.Bot.Stop();

                    return;
                }

                MagicBundle.Run(MagicBundleConfig);

                break;

            case AlchemyEngine.Attribute:
                if (AttributeBundle == null || AttributeBundleConfig == null)
                {
                    Log.Warn("[Alchemy] Configuration issue detected!");

                    Kernel.Bot.Stop();

                    return;
                }

                AttributeBundle.Run(AttributeBundleConfig);

                break;

            case AlchemyEngine.Manufacture:
                if (ManufactureBundle == null || ManufactureBundleConfig == null)
                {
                    Log.Warn("[Alchemy] Configuration issue detected!");

                    Kernel.Bot.Stop();

                    return;
                }

                ManufactureBundle.Run(ManufactureBundleConfig);

                break;

            case AlchemyEngine.Dismantle:
                if (DismantleBundle == null || DismantleBundleConfig == null)
                {
                    Log.Warn("[Alchemy] Configuration issue detected!");

                    Kernel.Bot.Stop();

                    return;
                }

                DismantleBundle.Run(DismantleBundleConfig);

                break;
        }
    }

    #endregion Methods
}

internal enum AlchemyEngine
{
    Enhance,
    Magic,
    Attribute,
    Manufacture,
    Dismantle
}