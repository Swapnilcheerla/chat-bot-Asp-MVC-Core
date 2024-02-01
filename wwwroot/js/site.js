window.onload = function () {
    var container = document.getElementById("chat-body");
    container.scrollTop = container.scrollHeight;
}

function displayBusyIndicator() {
    document.getElementById("loading").style.display = "block";
    document.getElementById("loading1").style.display = "block";
    document.getElementById("loading2").style.display = "block";
}

//"submitForm()" is called when the user clicks the "Send" button

   
function submitForm(questionNumber) {
    displayBusyIndicator();
    var question = "";
    if (questionNumber == "1")
    {
        question = "when was 2022 NY Slip Op 22419 decision held on"
    }
    else{
        question = "who was Judge for  2022 NY Slip Op 22419 decision";
    }
    // assign to question value to input variable
    document.getElementById("Prompt").value = question;
    // submit the form
    document.getElementById('searchForm').submit();

}