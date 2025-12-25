using RFOnline_CCG.ViewModels;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace RFOnline_CCG
{
    public partial class MainWindow : Window
    {
        private GameViewModel ViewModel => DataContext as GameViewModel;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new GameViewModel();
        }

        private void Card_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is CardViewModel card)
            {
                // Снимаем выделение с других карт
                foreach (var c in ViewModel.PlayerHand)
                {
                    c.IsSelected = false;
                }

                card.IsSelected = true;
                ViewModel.SelectedCard = card;
                ViewModel.SelectedCreature = null;

                e.Handled = true;
            }
        }

        private void Creature_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is CreatureViewModel creature)
            {
                // Снимаем выделение с других существ
                foreach (var c in ViewModel.PlayerField)
                {
                    c.IsSelected = false;
                }

                creature.IsSelected = true;
                ViewModel.SelectedCreature = creature;
                ViewModel.SelectedCard = null;

                e.Handled = true;
            }
        }

        private void Card_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (sender is FrameworkElement element && element.DataContext is CardViewModel card)
                {
                    // Начинаем перетаскивание
                    DragDrop.DoDragDrop(element, card, DragDropEffects.Move);
                }
            }
        }

        private void DropZone_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(CardViewModel)))
            {
                DropZone.Opacity = 0.5;
                DropZone.BorderThickness = new Thickness(3);
            }
        }

        private void DropZone_DragLeave(object sender, DragEventArgs e)
        {
            DropZone.Opacity = 0.2;
            DropZone.BorderThickness = new Thickness(1);
        }

        private void DropZone_Drop(object sender, DragEventArgs e)
        {
            DropZone.Opacity = 0.2;
            DropZone.BorderThickness = new Thickness(1);

            if (e.Data.GetData(typeof(CardViewModel)) is CardViewModel card)
            {
                ViewModel.PlayCardCommand.Execute(null);
            }
        }

        // Дополнительные методы для обработки клавиш
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (ViewModel == null) return;

            switch (e.Key)
            {
                case Key.Escape:
                    if (ViewModel.PauseMenuVisibility == Visibility.Visible)
                        ViewModel.ResumeGameCommand.Execute(null);
                    else if (ViewModel.GameBoardVisibility == Visibility.Visible)
                        ViewModel.ShowPauseMenuCommand.Execute(null);
                    break;

                case Key.Enter:
                    if (ViewModel.GameBoardVisibility == Visibility.Visible)
                        ViewModel.EndTurnCommand.Execute(null);
                    break;

                case Key.Space:
                    if (ViewModel.SelectedCard != null)
                        ViewModel.PlayCardCommand.Execute(null);
                    break;

                case Key.A:
                    if (ViewModel.SelectedCreature != null)
                        ViewModel.AttackCommand.Execute(null);
                    break;
            }
        }
    }
}