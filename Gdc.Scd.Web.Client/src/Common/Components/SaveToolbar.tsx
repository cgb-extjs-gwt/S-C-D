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

export class SaveToolbar<T extends SaveToolbarProps = SaveToolbarProps> extends React.Component<T, T> {
    constructor(props: T) {
        super(props);

        this.state = props;
    }

    public componentWillReceiveProps(nextProps: T) {
        if (this.state.isEnableClear != nextProps.isEnableClear || 
            this.state.isEnableSave != nextProps.isEnableSave) {
            this.setState(nextProps);
        }
    }

    public render() {
        const { isEnableClear, isEnableSave, onCancel, onSave } = this.state;
        const { children } = this.props;

        return(
            <Toolbar docked="bottom">
                <Button 
                    text="Cancel" 
                    flex={1} 
                    disabled={!isEnableClear}
                    handler={() => this.showClearDialog(onCancel)}
                />
                <Button 
                    text="Save" 
                    flex={1} 
                    disabled={!isEnableSave}
                    handler={() => this.showSaveDialog(onSave)}
                />
                {...this.getChildren()}
            </Toolbar>
        );
    }

    public enableClearButton(isEnable: boolean) {
        this.setState({ isEnableClear: isEnable });
    }

    public enableSaveButton(isEnable: boolean) {
        this.setState({ isEnableSave: isEnable });
    }

    public enable(isEnable: boolean) {
        this.enableClearButton(isEnable);
        this.enableSaveButton(isEnable);
    }

    protected getChildren() {
        return [this.props.children];
    }

    protected showSaveDialog(onSave? : () => void) {
        Ext.Msg.confirm(
          'Saving changes', 
          'Do you want to save the changes?',
          (buttonId: string) => buttonId == 'yes' && onSave && onSave()
        );
    }
    
    protected showClearDialog(onCancel?: () => void) {
        Ext.Msg.confirm(
            'Clearing changes', 
            'Do you want to clear the changes?',
            (buttonId: string) => buttonId == 'yes' && onCancel && onCancel()
        );
    }
}