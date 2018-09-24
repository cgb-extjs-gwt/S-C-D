import { DictFilter, DictFilterProps } from "./DictFilter";

export class DurationFilter extends DictFilter {
    public constructor(props: DictFilterProps) {
        super({
            ...props,
            getItems: x => x.getDurationTypes()
        });
    }
}