import * as React from "react";
import { SaveToolbar, SaveToolbarProps, SaveToolbarActions } from "../../Common/Components/SaveToolbar";
import { Button } from "@extjs/ext-react";

export interface SaveApprovalToollbarActions extends SaveToolbarActions {
    onApproval?()
}

export interface SaveApprovalToollbarProps extends SaveToolbarProps, SaveApprovalToollbarActions  {
    
}

export class SaveApprovalToollbar extends SaveToolbar<SaveApprovalToollbarProps> {
    protected getChildren() {
        const { isEnableSave, onApproval } = this.state;

        return [                
            <Button 
                key="approval"
                text="Save and send for approval" 
                flex={1} 
                disabled={!isEnableSave}
                handler={() => this.showSaveDialog(onApproval)}
            />
        ];
    }
}