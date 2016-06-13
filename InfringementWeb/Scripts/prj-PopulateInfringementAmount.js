if (!jQuery) { throw new Error("Bootstrap requires jQuery") }

$(document).ready(function () {
    $("#InfringementTypeId").on("change", function () {
        var infringementTypeId = $('#InfringementTypeId').val();
        var infringementTypeText = $("#InfringementTypeId option:selected").text(); 
            $.ajax({
                type: "POST",
                url: "/InfringementTypes/GetInfringementAmount",
                // The key needs to match your method's input parameter (case-sensitive).
                data: JSON.stringify({ infringementTypeId: infringementTypeId }),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (amount) {
                    
                    $("#Amount").val(amount);

                    if (infringementTypeText.toLowerCase().indexOf("other") >= 0)
                    {
                        $('#OtherInfringementType').show();
                        $('#lblOtherInfringementType').show();
                    }
                    else
                    {
                        $('#OtherInfringementType').hide();
                        $('#lblOtherInfringementType').hide();
                    }
                    
                },
                failure: function (errMsg) {
                    alert(errMsg);
                },
                error: function () {
                    $("#Amount").val("0.00");
                }
            });
        });
    });