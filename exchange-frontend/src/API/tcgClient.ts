import type { IPokemonCardDetail, IPokemonCardSearchResponse } from '../Interfaces/APIResponses/IPokemonCard';
import { apiPath, requestApi } from './apiClient';
import type { ApiResult } from './types';

const pokemonCardsPath = '/api/tcg/pokemon/cards';

export function searchPokemonCards(query: string, pageSize = 12): Promise<ApiResult<IPokemonCardSearchResponse>> {
  return requestApi<IPokemonCardSearchResponse>(apiPath(pokemonCardsPath, '', { query, pageSize }), {
    failureMessage: 'Pokemon card search failed',
    connectionMessage: 'Unable to connect to the TCG API.',
  });
}

export function getPokemonCard(id: string): Promise<ApiResult<IPokemonCardDetail>> {
  return requestApi<IPokemonCardDetail>(apiPath(pokemonCardsPath, `/${encodeURIComponent(id)}`), {
    failureMessage: 'Pokemon card detail request failed',
    connectionMessage: 'Unable to connect to the TCG API.',
  });
}
