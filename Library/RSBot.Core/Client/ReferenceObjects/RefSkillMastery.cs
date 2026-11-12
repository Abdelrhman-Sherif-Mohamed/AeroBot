using System;
using System.Drawing;
using RSBot.Core.Extensions;

namespace RSBot.Core.Client.ReferenceObjects;

public class RefSkillMastery : IReference<uint>
{
    public string Name => Game.ReferenceManager.GetTranslation(NameCode);

    public Image GetIcon()
    {
        Image bitmap = null;

        try
        {
            if (Game.MediaPk2 != null)
            {
                foreach (var candidate in new[] { Icon, FocusIcon })
                {
                    if (string.IsNullOrWhiteSpace(candidate) || candidate == "xxx")
                        continue;

                    var normalized = candidate.TrimStart('\\');
                    if (Game.MediaPk2.TryGetFile(normalized, out var file) && file != null)
                    {
                        bitmap = file.ToImage();
                        break;
                    }

                    if (!normalized.StartsWith("icon\\", StringComparison.OrdinalIgnoreCase) &&
                        Game.MediaPk2.TryGetFile($"icon\\{normalized}", out var prefixedFile) && prefixedFile != null)
                    {
                        bitmap = prefixedFile.ToImage();
                        break;
                    }
                }

                if (bitmap == null &&
                    Game.MediaPk2.TryGetFile("icon\\icon_default.ddj", out var defaultFile) &&
                    defaultFile != null)
                    bitmap = defaultFile.ToImage();
            }
        }
        catch
        {
        }
        finally
        {
            bitmap ??= new Bitmap(24, 24);
        }

        return bitmap;
    }

    #region Fields

    public uint ID;

    //public string Name;
    public string NameCode;

    public byte GroupNum;
    //public string Description;
    public string TabNameCode;
    public byte TabID;

    //public byte SkillToolTipType;

    //public byte WeaponType1;
    //public byte WeaponType2;
    //public byte WeaponType3;
    public string Icon;
    public string FocusIcon;

    #endregion Fields

    #region IReference

    public uint PrimaryKey => ID;

    public bool Load(ReferenceParser parser)
    {
        parser.TryParse(0, out ID);

        var offset = Game.ClientType >= GameClientType.Chinese ? 1 : 0;
        parser.TryParse(2 + offset, out NameCode);
        parser.TryParse(3 + offset, out GroupNum);
        parser.TryParse(5 + offset, out TabNameCode);
        parser.TryParse(6 + offset, out TabID);
        parser.TryParse(11 + offset, out Icon);
        parser.TryParse(12 + offset, out FocusIcon);

        if (string.IsNullOrWhiteSpace(Icon) || !Icon.EndsWith(".ddj", StringComparison.OrdinalIgnoreCase))
        {
            for (var col = 10; col <= 14; col++)
            {
                if (parser.TryParse(col, out string candidate) &&
                    !string.IsNullOrWhiteSpace(candidate) &&
                    candidate.EndsWith(".ddj", StringComparison.OrdinalIgnoreCase) &&
                    !candidate.EndsWith("_focus.ddj", StringComparison.OrdinalIgnoreCase))
                {
                    Icon = candidate;
                    break;
                }
            }
        }

        return true;
    }

    #endregion IReference
}

//Mastery ID: 257
//Mastery Name - Do Not Use: 비천검법101
//MasteryNameCode: UIIT_STT_MASTERY_VI
//GroupNum: 10
//Mastery Description ID: UIIT_STT_MASTERY_VI_EXPLANATION
//Tab Name Code: UIIT_CTL_WEAPON_SKILL
//Type (TabID): 0
//SkillToolTipType: 0
//Weapon Type 1: 2
//Weapon Type 2: 3
//Weapon Type 3: 0
//Mastery Icon: icon\skillmastery\china\mastery_sword.ddj
//Mastery Focus Icon: icon\skillmastery\china\mastery_sword_focus.ddj