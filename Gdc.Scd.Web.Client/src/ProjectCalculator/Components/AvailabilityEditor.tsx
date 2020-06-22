import * as React from "react";
import { Grid, Column, Toolbar, Button, GridCell } from "@extjs/ext-react";
import { DayOfWeek, AvailabilityProjCalc } from "../States/Project";
import { Model, Store, SelectionGridInfo } from "../../Common/States/ExtStates";

const HOUR_DATA_INDEX = 'hour'
const LAST_HOUR = 23

enum HourType {
    Standard,
    Premium,
    Vip
}

interface HourInfo {
    hour: number
    [dayName: string]: HourType
}

export interface AvailabilityEditorActions {
    onClose?()
    onSave?(availability: AvailabilityProjCalc)
}

export interface AvailabilityEditorProps extends AvailabilityEditorActions {
    availability: AvailabilityProjCalc
}

export interface AvailabilityEditorState {
    selectedAvailability: AvailabilityProjCalc
}

export class AvailabilityEditor extends React.PureComponent<AvailabilityEditorProps, AvailabilityEditorState> {
    private readonly dayNames: string[]
    private readonly store: Store<HourInfo>
    private readonly columns: any[]
    
    constructor(props: AvailabilityEditorProps) {
        super(props)

        this.dayNames = this.getDayNames();
        this.store = this.buildStore(this.dayNames);
        this.columns = [
            this.buildColumn(HOUR_DATA_INDEX, 'Hour'),
            ...this.dayNames.map(day => this.buildColumn(day, day, this.renderer))
        ];

        this.state = {
            selectedAvailability: null
        }
    }

    public render() {
        const { selectedAvailability } = this.state;

        return (
            <Grid 
                store={this.store} 
                flex={1}
                columnLines
                rowLines
                border
                selectable= {{
                    rows: true,
                    cells: true,
                    columns: true,
                    drag: true,
                }}
                onSelectionChange={this.onSelectionChange}
            >
                {...this.columns}
                
                <Toolbar layout="hbox" docked="bottom">
                    <Button text="Close" handler={this.onClose} flex={1}/>
                    <Button text="Save" handler={this.onSave} flex={1} disabled={!selectedAvailability}/>
                </Toolbar> 
            </Grid>
        )
    }

    private buildColumn(
        dataIndex: string, 
        title = dataIndex, 
        rendererFn: (value, record: Model<HourInfo>, dataIndex: string, cell) => any = null
    ) {
        return (
            <Column 
                key={dataIndex}
                dataIndex={dataIndex} 
                text={dataIndex} 
                menuDisabled
            >
                <GridCell renderer={rendererFn} bodyStyle={{ padding: '2px' }}/>
            </Column>
        );
    }

    private onSelectionChange = (
        grid,
        records: Model<HourInfo>[],
        selecting: boolean,
        selectionInfo: SelectionGridInfo
    )  => {
        const selectedAvailability: AvailabilityProjCalc = records.length == 0 
            ? null
            : {
                start: {
                    hour: records[0].data.hour,
                    day: selectionInfo.startCell.columnIndex - 1
                },
                end: {
                    hour: records[records.length - 1].data.hour,
                    day: selectionInfo.endCell.columnIndex - 1
                }
            }

        this.setState({ selectedAvailability });
    }

    private onClose = () => {
        const { onClose } = this.props;

        onClose && onClose();
    }

    private onSave = () => {
        const { onSave } = this.props;

        onSave && onSave(this.state.selectedAvailability);
    }

    private getBackground(hourType: HourType) {
        let background: string;
    
        switch (hourType) {
            case HourType.Standard:
                background = null;
                break;
            
            case HourType.Premium:
                background = 'rgba(224, 184, 184, 0.5)';
                break;

            case HourType.Vip:
                background = 'rgba(213, 217, 20, 0.5)';
                break;
        }

        return background;
    }

    private getBorderStyle(day: DayOfWeek, hour: number) {
        const borderStyle = '3px solid black';

        let borderTop: string = null;
        let borderRight: string = null;
        let borderBottom: string = null;
        let borderLeft: string = null;

        const { availability } = this.props;

        if (availability) {
            const { start, end } = availability;

            if (start && end && start.day <= day && day <= end.day) {
                if (hour == start.hour || hour == end.hour + 1) {
                    borderTop = borderStyle;
                } 

                if (hour == end.hour && hour == LAST_HOUR) {
                    borderBottom = borderStyle;
                }

                if (start.hour <= hour && hour <= end.hour) {
                    if (day == start.day) {
                        borderLeft = borderStyle;
                    }
    
                    if (day == end.day) {
                        borderRight = borderStyle;
                    }
                }
            }
        }

        return {
            'border-top': borderTop,
            'border-right': borderRight,
            'border-bottom': borderBottom,
            'border-left': borderLeft
        };

    }

    private renderer = (value, { data }: Model<HourInfo>, dataIndex: string, cell) => {
        cell.setStyle({ 
            ...this.getBorderStyle(DayOfWeek[dataIndex], data.hour),
            background: this.getBackground(data[dataIndex])
        });

        return ' ';
    }

    private getDayNames() {
        const dayOfWeeks: DayOfWeek[] = [];

        for (const day in DayOfWeek) {
            const value: any = DayOfWeek[day]

            if (typeof value == 'number') {
                dayOfWeeks.push(value);
            }
        }

        return dayOfWeeks.sort().map(day => DayOfWeek[day]);
    }

    private getHourType(day: DayOfWeek, hour: number) {
        let hourType: HourType

        if (day == DayOfWeek.Sunday) {
            hourType = HourType.Vip;
        } else if (day == DayOfWeek.Saturday) {
            hourType = HourType.Premium;
        } else if (8 <= hour && hour <= 17) {
            hourType = HourType.Standard;
        } else {
            hourType = HourType.Premium;
        }

        return hourType;
    }

    private buildHourInfos(dayNames: string[]) {
        const items: HourInfo[] = [];

        for (let hour = 0; hour <= LAST_HOUR; hour++) {
            const item: HourInfo = { hour: hour };

            for (const day of dayNames) {
                item[day] = this.getHourType(DayOfWeek[day], hour);
            }

            items.push(item);
        }

        return items;
    }

    private buildStore(dayNames: string[]) {
        return Ext.create('Ext.data.Store', {
            fields: [
                [HOUR_DATA_INDEX],
                ...dayNames
            ],
            data: this.buildHourInfos(dayNames)
        });
    }
}