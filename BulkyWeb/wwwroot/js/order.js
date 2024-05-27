var dataTable;


$(function () {

    var url = window.location.search;
    if (url.includes("inProcess")) {
        loadDataTable("inProcess");
    }
    else {
        if (url.includes("paymentPending")) {
            loadDataTable("paymentPending");
        }
        else {
            if (url.includes("completed")) {
                loadDataTable("completed");
            }
            else {
                if (url.includes("approved")) {
                    loadDataTable("approved");
                }
                else {
                    loadDataTable("all");
                }
            }
        }
    }
    
})



function loadDataTable(status) {

    dataTable = $("#tblData").DataTable({
        ajax: { url: "/Admin/Order/GetAll?status=" + status, type: "GET", dataSrc: "data" },
        columns: [
            { data: "id", "width": "5%" },
            { data: "name", "width": "20%" },
            { data: "phoneNumber","width":"15%"},
            { data: "applicationUser.email","width":"20%"},
            { data: "orderStatus", "width": "15%" },
            { data: "orderTotal", "width": "10%" },
            {
                data: "id",
                "render": function (data) {
                    return `
                        <div class="w-75 btn-group" role="group">
                            <a href="/Admin/Order/Details?orderId=${data}"  class="btn btn-primary mx-2"><i class="bi bi-pencil-square"></i> Details </a>
                        </div>`;
                },
                "width": "15%" 
            }
        ]
    });
}
