import * as React from "react";
import { HistoryWindowView, HistoryWindowViewProps } from "./HistoryWindowView";
import { Container, Button } from "@extjs/ext-react";
import { Position } from "../../Common/States/ExtStates";

export interface HistoryButtonViewProps {
    isEnabled: boolean
    flex: number
    windowPosition?: Position
    buidHistoryUrl?(): string
}

export interface HistoryButtonViewState {
    isVisibleWindow: boolean
}

export class HistoryButtonView extends React.Component<HistoryButtonViewProps, HistoryButtonViewState> {
    constructor(props) {
        super(props);

        this.state = {
            isVisibleWindow: false
        }
    }

    public render() {
        const { isEnabled, flex } = this.props;

        return (
            <Container flex={flex} layout={{type: 'vbox', align: 'center'}} >
                <Button 
                    text="History" 
                    disabled={!isEnabled}
                    handler={this.showHistoryWindow}
                />

                {
                    this.state.isVisibleWindow &&
                    this.buildHistoryWindow()
                }
            </Container>
        );
    }

    private buildHistoryWindow() {
        const { windowPosition, buidHistoryUrl } = this.props;

        const windowProps: HistoryWindowViewProps = {
            isVisible: this.state.isVisibleWindow,
            onClose: this.closeHistoryWindow,
            position: windowPosition,
            dataLoadUrl: buidHistoryUrl()
        };

        return (
            <HistoryWindowView {...windowProps} />
        );
    }

    private showHistoryWindow = () => {
        this.setState({
            isVisibleWindow: true
        });
    }

    private closeHistoryWindow = () => {
        this.setState({ 
            isVisibleWindow: false 
        });
    }
}