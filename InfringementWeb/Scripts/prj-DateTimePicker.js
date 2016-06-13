    $(function () {
        $('#datetimepicker1').datetimepicker({
            format: "DD-MM-YYYY HH:mm",
            defaultDate: moment()
        });
        
        $('#datepicker1').datetimepicker({
            format: "DD-MM-YYYY",
            defaultDate: moment()
        });

        $('#userdatepicker1').datetimepicker({
            format: "DD-MM-YYYY",
            defaultDate: moment()
        });

        $('#datepicker2').datetimepicker({
            format: "DD-MM-YYYY",
            defaultDate: moment()
        });

        $("#RecPerPage").on("change", function () {
            //debugger;
            var recordsperpage = $('#RecPerPage').val();
            window.location = "/Infringements/Index?page=1&sortOrder=IncidentTime&lstpagesize=" + recordsperpage;
        });
    });