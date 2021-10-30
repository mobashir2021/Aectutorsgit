function SearchRegisterCriteria() {

    var self = this;

    self.CentreId = ko.observable("");
    self.SessionId = ko.observable("");
    self.FromDate = ko.observable("");
    self.ToDate = ko.observable("");
    self.Status = ko.observable("");
    self.PageSize = ko.observable(10);
    self.PageIndex = ko.observable(0);
}


function RegisterSearchViewModel(registerSearchViewModel, allSessions) {
    
    var self = this;
    self.Registers = ko.observableArray(registerSearchViewModel.Registers);
    self.SearchRegistersUrl = registerSearchViewModel.SearchRegistersUrl;
    self.AddRegisterUrl = registerSearchViewModel.AddRegisterUrl;
    self.AllSessions = allSessions;

    self.edit = function (register) {
        location.href = register.EditUrl;
        //alert("called");
    };

    self.delete = function(register) {
        if(confirm("Are you sure you would like to delete this student")) {
            location.href = register.DeleteUrl;
        }
    };

    self.AddRegister = function () {
        location.href = self.AddRegisterUrl;
    };

    self.SearchRegisters = function () {
        var criteria = ko.toJS(self.SearchCriteria);
        $.post(self.SearchRegistersUrl, criteria, function (returnedData) {
            // This callback is executed if the post was successful    
            self.Registers(returnedData.Registers);
            self.Pager(new Pager(criteria, returnedData.MaxPageIndex, self.MoveToPage));
        });

    };

    self.MoveToPage = function () {
        var criteria = self.Pager().SearchCriteria;
        $.post(self.SearchRegistersUrl, criteria, function (returnedData) {
            // This callback is executed if the post was successful    
            self.Registers(returnedData.Registers);
        });
    };
    
    self.SearchCriteria = new SearchRegisterCriteria();
    self.SessionsByCenter = ko.computed(function() {
                var center = self.SearchCriteria.CentreId();
                
                return ko.utils.arrayFilter(self.AllSessions, function(s) {
                    return s.Location == center;
                });
            }, self);

    
    self.Pager = ko.observable(new Pager(ko.toJS(self.SearchCriteria), registerSearchViewModel.MaxPageIndex, self.MoveToPage));
     $(".date").datepicker({ dateFormat: 'dd/mm/yy', changeMonth: true, changeYear: true, yearRange: '1940:2022' });
    
}