import { shomMask, hideMask } from "./MaskHelper";

export const handleRequest = (promise: Promise<any>) => {
    shomMask();

    return promise.then(hideMask).catch(error => { 
        hideMask();
        Ext.Msg.alert('Error', 'Request failed');
    });
}