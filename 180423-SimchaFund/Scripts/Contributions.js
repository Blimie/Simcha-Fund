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