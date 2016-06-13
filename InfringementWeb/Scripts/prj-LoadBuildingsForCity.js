if (!jQuery) { throw new Error("Bootstrap requires jQuery") }

$(document).ready(function () {

   

        $("#CityId").on("change", function() {
            var cityId = $('#CityId').val();
            $.ajax({
                type: "POST",
                url: "/Building/GetBuildingsForCity",
                // The key needs to match your method's input parameter (case-sensitive).
                data: JSON.stringify({ cityId: cityId }),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (buildings) {
                   // debugger;
                    // buildings is your JSON array
                    //var $select = $('#ParkingLocationId');
                    //($select).empty();
                    //$.each(buildings, function (i, building) {
                    //    $('<option>', {
                    //        value: building.Key
                    //    }).html(building.Value).appendTo($select);
                    //});

                    var markup = "";
                    for (var x = 0; x < buildings.length; x++) {
                        markup += "<option value=" + buildings[x].Key + ">" + buildings[x].Value + "</option>";
                    }
                    $("#ParkingLocationId").html(markup).show();

                },
                failure: function (errMsg) {
                    alert(errMsg);
                }
            });
        });

    //loading the make based on model
        $("#MakeId").on("change", function () {
          //debugger;
            var makeId = $('#MakeId').val();
            var makename = $("#MakeId option:selected").text();
            $.ajax({
                type: "POST",
                url: "/Makes/GetModelsbyMake",
                // The key needs to match your method's input parameter (case-sensitive).
                data: JSON.stringify({ makeId: makeId }),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (models) {
                    
                    var markup = "";
                    markup += "<option value=''>Select Model</option>";
                   // markup += "<option value=" + models[x].Key + ">" + models[x].Value + "</option>";
                    for (var x = 0; x < models.length; x++) {
                        markup += "<option value=" + models[x].Key + ">" + models[x].Value + "</option>";
                    }
                    $("#ModelId").html(markup).show();

                },
                failure: function (errMsg) {
                    alert(errMsg);
                }
            });

            if (makename.toLowerCase().indexOf("other") >= 0 )
            {
                
                    $('#OtherMake').show();
                    $('#lblOtherMake').show();
             }
            else {
                $('#OtherMake').hide();
                $('#lblOtherMake').hide();
            }
        });

        $("#ModelId").on("change", function () {
            //debugger;
            var makeId = $('#ModelId').val();
            var modelname = $("#ModelId option:selected").text();

            if (modelname.toLowerCase().indexOf("other") >= 0) {

                $('#OtherModel').show();
                $('#lblOtherModel').show();
            }
            else {
                $('#OtherModel').hide();
                $('#lblOtherModel').hide();
            }
        });

        $('#IsOtherMake').click(function () {
            //debugger;
            if ($(this).is(':checked')) {
                $('#OtherMake').show();
                $('#OtherModel').show();
                $('#lblOtherMake').show();
                $('#lblOtherModel').show();
            }
            else
            {
                $('#OtherMake').hide();
                $('#OtherModel').hide();
                $('#lblOtherMake').hide();
                $('#lblOtherModel').hide();
            }
        });

        $("#RecPerPage").on("change", function () {
                //debugger;
            var recno = $('#RecPerPage').val();
                
                
            });
    });