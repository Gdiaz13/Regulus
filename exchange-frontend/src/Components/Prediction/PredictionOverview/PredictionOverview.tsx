import { formatPercentage } from '../../../lib/formatters';
import type { IAiCategoryPrediction, IAiOverview } from '../../../Interfaces/APIResponses/IPrediction';
import PredictionCard from '../PredictionCard/PredictionCard';
import styles from './PredictionOverview.module.css';

// The full RegulasCoreAI overview: a top-line summary plus one block per category AI.
export default function PredictionOverview({ overview }: { overview: IAiOverview }) {
  return (
    <section className={styles.overview}>
      <CommanderSummary overview={overview} />
      {overview.categories.map((category) => <CategoryBlock key={category.category} category={category} />)}
    </section>
  );
}

function CommanderSummary({ overview }: { overview: IAiOverview }) {
  return (
    <header className={styles.commander}>
      <p className={styles.eyebrow}>{overview.modelName} v{overview.modelVersion}</p>
      <p className={styles.summary}>{overview.summary}</p>
    </header>
  );
}

function CategoryBlock({ category }: { category: IAiCategoryPrediction }) {
  return (
    <div className={styles.category}>
      <CategoryHeader category={category} />
      <div className={styles.grid}>
        {category.predictions.map((prediction) => <PredictionCard key={cardKey(prediction)} prediction={prediction} />)}
      </div>
    </div>
  );
}

function CategoryHeader({ category }: { category: IAiCategoryPrediction }) {
  return (
    <header className={styles.categoryHeader}>
      <h2 className={styles.categoryTitle}>{category.category}</h2>
      <p className={styles.categorySummary}>{category.summary}</p>
      <p className={styles.categoryStats}>
        Avg confidence {formatPercentage(category.averageConfidence * 100)} - Avg risk {formatPercentage(category.averageRisk * 100)}
      </p>
    </header>
  );
}

function cardKey(prediction: { assetId: string; modelName: string }) {
  return `${prediction.modelName}:${prediction.assetId}`;
}
