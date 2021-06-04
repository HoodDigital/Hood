import { Modal } from 'bootstrap';
import { Alerts, Response } from './hood';

let modal = $('#exampleModal');
modal.addClass('dewsh');

var myModal = new Modal(document.getElementById('exampleModal'));
//myModal.show();

Alerts.error("This is working!", "Great!!!");
Alerts.warning("This is working!", "Great!!!");
Alerts.success("This is working!", "Great!!!");
Alerts.message("This is working!", "Great!!!");
Alerts.prompt({ html: "What is this?", title: "Quick Question".htmlEncode() }, function (result) {
    if (!result.isDismissed) {
        Alerts.message(`We heard ${result.value}`, "Great!!!");
    } else {
        Alerts.message(`Man no respondy...`, "Uh oh...");
    }
});
