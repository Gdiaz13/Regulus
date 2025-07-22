import { ClipLoader } from 'react-spinners';

type Props = {
    isLoading?: boolean;
};

const Spinner = ({ isLoading = true}: Props) => {
  return (<>
    <div id="loading-spinner">
        <ClipLoader
            color="#FFD700"
            loading={isLoading}
            size={150}
            aria-label="Loading Spinner"
            data-testid="loader" 
            />
    </div>
    </>
  
  )
}

export default Spinner