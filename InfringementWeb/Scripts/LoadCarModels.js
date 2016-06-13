if (!jQuery) { throw new Error("Bootstrap requires jQuery") }

$(document).ready(function () {
        $("#CarModel").on("keyup", function() {
            var makeId = $('#MakeId').val();
            var val = $("#CarModel").val();
            $.ajax({
                type: "POST",
                url: "/CarModels/GetModels",
                // The key needs to match your method's input parameter (case-sensitive).
                data: JSON.stringify({ makeId: makeId, modelName: val }),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (data) {
                    $("#CarModel").autocomplete({
                        source: data
                    });
                },
                failure: function (errMsg) {
                    alert(errMsg);
                }
            });
        });
    });