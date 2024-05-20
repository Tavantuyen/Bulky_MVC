var dataTable;
$(function () {
    
    loadDataTable();
    
})



function loadDataTable() {

    //lưu ý columns= phải bằng với số cọt của thead trong file index product
    dataTable = $("#tblData").DataTable({
            "ajax": { url: "/Admin/Product/Getall", type: "GET", dataSrc:"data"},
            "columns":[
                    { data: "id", "width": "1%" },
                    { data: "title", "width": "10%" },
                    { data: "description", "width": "35%" },
                    { data: "isbn", "width": "5%" },
                    { data: "author", "width": "10%" },
                    { data: "category.name", "width": "15%" },
                    { data: "listPrice", "width": "1%" },
                    { data: "price", "width": "1%" },
                    { data: "price50", "width": "1%" },
                    { data: "price100", "width": "1%" },
                    {
                            data: "id",
                            "render": function (data) {
                                return `<div class="w-75 btn-group" role="group">
                                            <a href="/Admin/Product/Upsert?id=${data}" class="btn btn-primary mx-2">Edit <i class="bi bi-pencil-square"></i></a>
                                            <a onclick=Delete("/Admin/Product/Delete/${data}") class="btn btn-danger mx-2">Delete <i class="bi bi-trash-fill"></i></a>
                                        </div>`;
                            },
                            "width": "20%"
                     }
            ]
    });
   
}


function Delete(url) {
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!" 
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url, //url là /Admin/Product/Delete/${data}
                type: "DELETE", // gọi Delete có method DELETE bên ProductController
                success: function (data) {
                    dataTable.ajax.reload(),  //load lại trang 
                    toastr.success(data.message); // thông báo thành công (nhận từ json)
                }
            });
        }
    });
}