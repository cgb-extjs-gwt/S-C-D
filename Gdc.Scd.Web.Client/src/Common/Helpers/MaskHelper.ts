import { ROOT_LAYOUT_ID } from "../../Layout/Components/Layout";

const getLayout = () => (<any>Ext.getCmp(ROOT_LAYOUT_ID));

export const showMask = (cmp=getLayout()) => {
    cmp && cmp.setMasked({ xtype: "loadmask" });
}

export const hideMask = (cmp=getLayout()) => {
    cmp && cmp.setMasked(false);
}