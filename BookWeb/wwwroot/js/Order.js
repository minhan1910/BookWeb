var dataTable;

$(document).ready(function () {
    var url = window.location.search;

    var statusLabelArr = [ "inprocess", "pending", "approved", "completed", "all"];    

    // ?status=inprocess | ?status=pending

    if (url.includes("status")) {
        statusLabelArr
            .filter(statusLabel => url.includes(statusLabel))
            .forEach(statusLabel => {
                loadDataTable(statusLabel);
            });
    } else {
        loadDataTable("all");
    }

});

function loadDataTable(status = "all") {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/admin/order/getall?status=' + status },
        "columns": [
            { data: 'id', "width": "5%"},
            { data: 'name', "width": "25%"},
            { data: 'phoneNumber', "width": "20%"},
            { data: 'applicationUser.email', "width": "20%"},            
            { data: 'orderStatus', "width": "10%" },
            { data: 'orderTotal', "width": "10%" },
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">
                         <a href="/admin/order/details?orderId=${data}" class="btn btn-primary mx-2"> <i class="bi bi-pencil-square"></i> Edit</a>               
                    `
                 },
                "width": "10%"
            }
        ]
    });
}

