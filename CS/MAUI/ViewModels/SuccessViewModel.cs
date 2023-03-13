using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAUI.ViewModels {
    public class SuccessViewModel : BaseViewModel {
        public string Header {
            get;
            set;
        }
        public string Info {
            get;
            set;
        }
        public SuccessViewModel() {
            Header = "Success!";
            Info = "Now you can send requests to the WebAPI service to get data based on your permissions \nPlease see the 'Role-based data access' example to learn more";
        }
    }
}
