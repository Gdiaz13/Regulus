import styles from './RatioList.module.css'
import { renderRatioListCells } from './helpers/renderRatioListCells'
import type { DataConfig } from '../Table/types';

type Props<T> = {
  config: DataConfig<T>[];
  data: T;
}

const RatioList = <T,>({config, data}: Props<T>) => {
  return (
    <div className={styles.ratioListWrapper}>
      <ul className={styles.ratioList}>
        {renderRatioListCells(config, data)}
      </ul>
    </div>
  )
};

export default RatioList
