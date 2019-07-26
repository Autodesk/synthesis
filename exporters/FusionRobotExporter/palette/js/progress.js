// Handles the receiving of data from Fusion
window.fusionJavaScriptHandler =
    {
        handle: function (action, data)
        {
            try
            {
                if (action == 'progress')
                {
                    var percent = parseFloat(data);

                    document.getElementById('progress').value = percent * 100;

                    // Remove error message if present
                    document.getElementById('error').style.display = "none";
                    document.getElementById('status').style.display = "";

                    if (document.getElementById('progress').classList.contains('error'))
                        document.getElementById('progress').classList.remove('error');

                    console.log(data);
                }
                else if (action == 'error')
                {
                    console.log("Error: " + data);
                    document.getElementById('error').innerHTML = data;
                    document.getElementById('error').style.display = "";
                    document.getElementById('status').style.display = "none";
                    document.getElementById('progress').value = 100;
                    document.getElementById('progress').classList.add('error');
                    lastProgress = 1;
                }
                else if (action == 'success')
                {
                    console.log("Error: " + data);
                    document.getElementById('status').innerHTML = "Export Successfully Completed! (This will auto-close in 5s)<br />Your robot can be found in " + data;
                    document.getElementById("progress").style.visibility = "hidden";

                    setTimeout(resetView, 6000);
                    
                }

                else if (action == 'debugger')
                {
                    debugger;
                }
                else
                {
                    return 'Unexpected command type: ' + action;
                }
            }
            catch (e)
            {
                console.log('Exception while excecuting \"' + action + '\":');
                console.log(e);
            }
            return 'OK';
        }
    };

// Once the progress window is hidden, restore to previous look
function resetView() {
    document.getElementById("status").innerHTML = "Exporting robot..." + "<br />" + "Please do not leave the Synthesis workspace or close Fusion.";
    document.getElementById("progress").style.visibility = "visible";
    
}