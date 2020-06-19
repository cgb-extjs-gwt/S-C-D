import { showMask, hideMask } from "./MaskHelper";

let requestCount = 0

export const handleRequest = (promise: Promise<any>) => {
    if (requestCount == 0) {
        showMask();
    }

    requestCount++;

    return promise.then(() => {
                        requestCount--;

                        if (requestCount == 0) {
                            hideMask();
                        }
                   }) 
                   .catch(error => {
                        requestCount--;
                        hideMask();
                        Ext.Msg.alert('Error', 'Request failed');
                        console.error(error);
                   });
}