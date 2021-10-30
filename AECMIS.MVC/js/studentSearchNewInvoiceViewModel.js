
function SearchCriteria() {
    var self = this;

    self.FirstName = ko.observable("");
    self.LastName = ko.observable("");
    self.StudentNo = ko.observable("");
    self.Curriculum = ko.observable("");
    self.ActiveOnly = ko.observable(true);
    self.PageSize = ko.observable(10);
    self.PageIndex = ko.observable(0);

}



function StudentSearchViewModel(studentSearchViewModel, callback) {
    var self = this;
    self.Students = ko.observableArray(studentSearchViewModel.Students);
    self.SearchStudentsUrl = studentSearchViewModel.SearchStudentsUrl;
    self.AddStudentUrl = studentSearchViewModel.AddStudentUrl;
    self.CanAddNewStudent = studentSearchViewModel.CanAddNewStudent;
    self.AreStudentsSelectable = studentSearchViewModel.AreStudentsSelectable;
    self.SelectedStudents = ko.observableArray([]);
    self.CallBack = callback;
    //self.CurrentStudent = ko.observable(new CurrentStudentDetails({}));

    


    // self.SelectedStudents.subscribe(function(newValue) {
    //   alert(newValue);
    //        var newSelectedUserNames = newValue;
    //        var newSelectedUsers = [];
    //        ko.utils.arrayForEach(newSelectedUserNames, function(userName) {
    //            var selectedUser = ko.utils.arrayFirst(self.AllUsers(), function(user) {
    //                return (user.userName() === userName);
    //            });
    //            newSelectedUsers.push(selectedUser);
    //        });
    //        self.SelectedUsers(newSelectedUsers);
    //});

    self.edit = function (student) {
        location.href = student.EditUrl;
        //alert("called");
    };

    self.delete = function (student) {
        if (confirm("Are you sure you would like to delete this student")) {
            location.href = student.DeleteUrl;
        }
    };

    self.AddStudent = function () {
        location.href = self.AddStudentUrl;
    };

    self.SearchStudents = function () {
        var criteria = ko.toJS(self.SearchCriteria);
        $.post(self.SearchStudentsUrl, criteria, function (returnedData) {
            // This callback is executed if the post was successful    
            self.Students(returnedData.Students);
            self.Pager(new Pager(criteria, returnedData.MaxPageIndex, self.MoveToPage));
        });

    };

    self.MoveToPage = function () {
        var criteria = self.Pager().SearchCriteria;
        $.post(self.SearchStudentsUrl, criteria, function (returnedData) {
            // This callback is executed if the post was successful    
            self.Students(returnedData.Students);
        });
    };

    self.SearchCriteria = new SearchCriteria();
    self.Pager = ko.observable(new Pager(ko.toJS(self.SearchCriteria), studentSearchViewModel.MaxPageIndex, self.MoveToPage));

}