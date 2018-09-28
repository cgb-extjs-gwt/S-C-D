import * as React from "react";
import { Toolbar, Button } from "@extjs/ext-react";

export interface SaveToolbarActions {
    onCancel?()
    onSave?()
}

export interface SaveToolbarProps extends SaveToolbarActions {
    isEnableClear: boolean
    isEnableSave: boolean
}

export class SaveToolbar extends React.Component<SaveToolbarProps> {
    public render() {
        const { isEnableClear, isEnableSave, children } = this.props;

        return(
            <Toolbar docked="bottom">
                <Button 
                    text="Cancel" 
                    flex={1} 
                    disabled={!isEnableClear}
                    handler={() => this.showClearDialog()}
                />
                <Button 
                    text="Save" 
                    flex={1} 
                    disabled={!isEnableSave}
                    handler={() => this.showSaveDialog(false)}
                />
                {children}
            </Toolbar>
        );
    }

    private showSaveDialog(forApproval: boolean) {
        const { onSave } = this.props;
    
        Ext.Msg.confirm(
          'Saving changes', 
          'Do you want to save the changes?',
          (buttonId: string) => buttonId == 'yes' && onSave && onSave()
        );
    }
    
    private showClearDialog() {
        const { onCancel } = this.props;

        Ext.Msg.confirm(
            'Clearing changes', 
            'Do you want to clear the changes?',
            (buttonId: string) => buttonId == 'yes' && onCancel && onCancel()
        );
    }
}