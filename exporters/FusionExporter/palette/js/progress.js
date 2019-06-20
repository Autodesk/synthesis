var lastProgress = 0;

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

                    // Instantly reset progress bar if the progress is decreasing
                    if (percent < lastProgress)
                        document.getElementById('progress').classList.add('notransition');

                    document.getElementById('progress').value = percent * 100;

                    // Remove error message if present
                    document.getElementById('error').style.display = "none";
                    document.getElementById('status').style.display = "";

                    if (document.getElementById('progress').classList.contains('error'))
                        document.getElementById('progress').classList.remove('error');

                    // Add back the transition a moment later so that the view has time to reset
                    if (percent < lastProgress)
                        setTimeout(function ()
                        {
                            document.getElementById('progress').classList.remove('notransition')
                        }, 1);

                    // Update last progress value
                    lastProgress = percent;

                    console.log(data);
                }
                else if (action == 'error')
                {
                    console.log("Error: " + data);
                    document.getElementById('error').innerHTML = data
                    document.getElementById('error').style.display = "";
                    document.getElementById('status').style.display = "none";
                    document.getElementById('progress').value = 100;
                    document.getElementById('progress').classList.add('error');
                    lastProgress = 1;
                }
                else if (action == 'success')
                {
                    console.log("Error: " + data);
                    document.getElementById('status').innerHTML = "Export Successfully Completed! (This will auto-close in 5s)"
                    document.getElementById('progress').style.display = "none";

                    lastProgress = 1;
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
