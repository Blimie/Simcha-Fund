$(function () {
    $("#add-contributor").on('click', function () {
        $("#add-contributor-modal").modal();
    });
    $(".deposit").on('click', function () {
        const contributorId = $(this).data('contributor-id');  
        $("#deposit-contributor-id-hidden").val(contributorId);
        $("#deposit-modal").modal();
    });
    $(".edit").on('click', function () {
        const contributorId = $(this).data('contributor-id');  
        const firstName = $(this).data('first-name');
        const lastName = $(this).data('last-name');
        const cellNumber = $(this).data('cell-number');
        const dateCreated = $(this).data('date-created');
         
        $("#contributor-id-hidden").val(contributorId);
        $("#firstName").val(firstName);
        $("#lastName").val(lastName);
        $("#cellNumber").val(cellNumber);
        $("#dateCreated").val(dateCreated);    

        $("#edit-modal").modal();
    });  
    $("#searchInput").on("keyup", function () {
        let value = $(this).val().toLowerCase();
        $("#Table tr").filter(function () {
            $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
        });
    });
    $("#clear").on('click', function () {    
        $("#searchInput").val("");
        $('#searchInput').trigger("keyup");
    });
});    