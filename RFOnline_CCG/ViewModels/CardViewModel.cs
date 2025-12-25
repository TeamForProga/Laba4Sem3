using RFCardGame.Core;
using RFOnline_CCG.Resources;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RFOnline_CCG.ViewModels
{
    public class CardViewModel : BaseViewModel
    {
        private ICard _card;
        private bool _isSelected;

        public string Name => _card.Name;
        public int Cost => _card.Cost;

        public ImageSource CardImage
        {
            get
            {
                return ImageManager.GetCardImage(_card.Name, _card.Type);
            }
        }
        public ImageSource FactionImage
        {
            get
            {
                string factionName = _card.Faction.ToString();
                return new BitmapImage(new Uri($"/Images/Factions/{factionName}.png", UriKind.RelativeOrAbsolute));
            }
        }
        public Brush FactionColor
        {
            get
            {
                return _card.Faction switch
                {
                    Faction.Accretia => new SolidColorBrush(Color.FromRgb(255, 51, 51)),
                    Faction.Bellato => new SolidColorBrush(Color.FromRgb(51, 153, 255)),
                    Faction.Cora => new SolidColorBrush(Color.FromRgb(204, 51, 255)),
                    _ => Brushes.White
                };
            }
        }

        public bool IsCreature => _card is ICreatureCard;
        public bool IsSpell => _card is ISpellCard;
        public bool IsArtifact => _card is IArtifactCard;

        // Для существ
        public int Attack => (_card as ICreatureCard)?.Attack ?? 0;
        public int Health => (_card as ICreatureCard)?.CurrentHealth ?? 0;
        public int MaxHealth => (_card as ICreatureCard)?.MaxHealth ?? 0;
        public string HealthText => $"{Health}/{MaxHealth}";

        // Для заклинаний
        public string SpellType => (_card as ISpellCard)?.Subtype.ToString() ?? "";
        public int Power => (_card as ISpellCard)?.Power ?? 0;
        public string TargetType => (_card as ISpellCard)?.TargetType ?? "";

        public string EffectText => _card.EffectText;
        public string Lore => _card.Lore;

        public bool IsSelected
        {
            get => _isSelected;
            set => SetField(ref _isSelected, value);
        }

        public ICard GetCard() => _card;

        public CardViewModel(ICard card)
        {
            _card = card;
        }
    }
}