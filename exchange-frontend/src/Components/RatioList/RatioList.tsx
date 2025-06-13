import React from 'react'

import styles from './RatioList.module.css'
import { renderRatioListCells } from './helpers/renderRatioListCells'

type Props = {
  config: any;
  data: any;
}



const RatioList = ({config, data}: Props) => {
  return (
    <div className={styles.ratioListWrapper}>
      <ul className={styles.ratioList}>
        {renderRatioListCells(config, data)}
      </ul>
    </div>
  )
};

export default RatioList