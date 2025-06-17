
import styles from './Sidebar.module.css'
import { Link } from 'react-router-dom'
import { FcHome } from "react-icons/fc";
import { FcMoneyTransfer } from "react-icons/fc";
import { FaScaleBalanced } from "react-icons/fa6";



type Props = {}

const Sidebar = (props: Props) => {
  return (
    <nav className={styles.sidebar}>
      <div className={styles.sidebarContent}>
        <div className={styles.sidebarNav}>
            
          <Link to ="company-profile" className={styles.link}>
            <FcHome />
            <h6 className={styles.linkLabel}>Company Profile</h6>
          </Link>

          <Link to ="income-statement" className={styles.link}>
            <FcMoneyTransfer />
            <h6 className={styles.linkLabel}>Income Statement</h6>
          </Link>

          <Link to ="balance-sheet" className={styles.link}>
            <FaScaleBalanced />
            <h6 className={styles.linkLabel}>Balance Sheet</h6>
          </Link>
          

          
        </div>
      </div>
    </nav>
  )
}

export default Sidebar