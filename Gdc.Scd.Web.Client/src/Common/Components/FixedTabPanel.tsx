import * as React from 'react';
import { TabPanel, Panel, Container } from "@extjs/ext-react";

export interface FixedTabPanelProps {
    activeTab?: number;
    [key: string]: any;
}

// TabPanel from Ext.js wasn't work normaly.
const FixedTabPanel: React.StatelessComponent<FixedTabPanelProps> = props => {
    const activeTab = props.activeTab | 0;
    const tabTitles = React.Children.map(
        props.children, 
        tab => (tab as React.ReactElement<any>).props.title);

    return (
        <Panel>
            <TabPanel {...props}>
                {tabTitles && tabTitles.map((title, index) => <Container key={index} title={title}/>)}
            </TabPanel>
            <Panel>
                {React.Children.toArray(props.children)[activeTab]}
            </Panel>
        </Panel>
    );
};

export default FixedTabPanel;