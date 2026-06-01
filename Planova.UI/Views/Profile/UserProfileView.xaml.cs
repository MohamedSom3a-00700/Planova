using System.Windows.Controls;
using Planova.UI.ViewModels;

namespace Planova.UI.Views.Profile;

public partial class UserProfileView : UserControl
{
    public UserProfileView(UserProfileViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
