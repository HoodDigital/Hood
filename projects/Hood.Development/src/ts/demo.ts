import { Modal } from 'bootstrap';
import { Alerts, Response } from './hood';

let modal = $('#exampleModal');
modal.addClass('dewsh');

var myModal = new Modal(document.getElementById('exampleModal'));
//myModal.show();

Alerts.Error("This is working!", "Great!!!");
Alerts.Warning("This is working!", "Great!!!");
Alerts.Success("This is working!", "Great!!!");
Alerts.Message("This is working!", "Great!!!");
Alerts.Prompt({ html: "What is this?", title: "Quick Question".htmlEncode() }, function (result) {
    if (!result.isDismissed) {
        Alerts.Message(`We heard ${result.value}`, "Great!!!");
    } else {
        Alerts.Message(`Man no respondy...`, "Uh oh...");
    }
});
