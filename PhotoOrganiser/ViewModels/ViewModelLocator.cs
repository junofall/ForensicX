using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForensicX.ViewModels
{
    public class ViewModelLocator
    {
        private HomeViewViewModel _homeViewViewModel;

        public HomeViewViewModel HomeViewViewModel
        {
            get
            {
                if (_homeViewViewModel == null)
                {
                    System.Diagnostics.Debug.WriteLine("ViewModelLocator: Instantiating new HomeViewViewModel.");
                    _homeViewViewModel = new HomeViewViewModel();
                }
                if(_homeViewViewModel.EvidenceItems.Count > 0) 
                {
                    System.Diagnostics.Debug.WriteLine("ViewModelLocator: HomeViewViewModel has evidence items attached!");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ViewModelLocator: HomeViewViewModel does not have evidence items attached!");
                }
                return _homeViewViewModel;
            }
        }
    }
}
