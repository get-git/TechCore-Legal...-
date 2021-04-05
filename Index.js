$(document).ready(function () {
    //alert('hello my friend')
    console.log('hello my friend');

    $.ajax({
        type: 'GET',
        url: '/api/Proxemics2/testing',
        cache: false,
        success: function (result) {
            console.log(result)
            //document.getElementById("InternsTable").innerHTML = result;
        }
    });
});


$.ajax({
    type: 'GET',
    url: '/api/Proxemics2/PopulateUsersAverageAirflow',
    cache: false,
    success: function (result) {
        console.log(result)
        document.getElementById("table").innerHTML = result;
    }
});