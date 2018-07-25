#ifndef _BOUNDS_CHECKED_ARRAY_HPP_
#define _BOUNDS_CHECKED_ARRAY_HPP_

#include <array>
#include <functional>
#include "util.hpp"

namespace hel{
    /**
     * \struct BoundsCheckedArray
     * \brief Array type with bounds checking and helpful operators
     */
    template<typename T, std::size_t LEN>
    struct BoundsCheckedArray{
    private:

        /**
         * \var std::array<T, LEN> internal
         * \brief The internal data array for this wrapper
         */

        std::array<T, LEN> internal;

    public:

        /**
         * \fn constexpr const T& at(std::size_t pos)const
         * \brief Returns a reference to the element at pos with bounds checking
         * \param The position of the element to return
         * \return A reference to the requested element
         */

        constexpr const T& at(std::size_t pos)const{
            if(pos < 0 || pos >= LEN){
                throw std::out_of_range("Exception: array index out of bounds: index " + std::to_string(pos) + " in array of size " + std::to_string(LEN));
            }
            return internal[pos];
        }

        /**
         * \fn constexpr T& at(std::size_t pos)
         * \brief Returns a reference to the element at pos with bounds checking
         * \param The position of the element to return
         * \return A reference to the requested element
         */

        constexpr T& at(std::size_t pos){
            if(pos < 0 || pos >= LEN){
                throw std::out_of_range("Exception: array index out of bounds: index " + std::to_string(pos) + " in array of size " + std::to_string(LEN));
            }
            return internal[pos];
        }

        /**
         * \fn constexpr const T& operator[](std::size_t pos)const
         * \brief Returns a reference to the element at pos with bounds checking
         * \param The position of the element to return
         * \return A reference to the requested element
         */

        constexpr const T& operator[](std::size_t pos)const{
            if(pos < 0 || pos >= LEN){
                throw std::out_of_range("Exception: array index out of bounds: index " + std::to_string(pos) + " in array of size " + std::to_string(LEN));
            }
            return internal[pos];
        }

        /**
         * \fn constexpr T& operator[](std::size_t pos)
         * \brief Returns a reference to the element at pos with bounds checking
         * \param The position of the element to return
         * \return A reference to the requested element
         */

        constexpr T& operator[](std::size_t pos){
            if(pos < 0 || pos >= LEN){
                throw std::out_of_range("Exception: array index out of bounds: index " + std::to_string(pos) + " in array of size " + std::to_string(LEN));
            }
            return internal[pos];
        }

        /**
         * \fn constexpr const T& front()const
         * \brief Returns a reference to the first element
         * \return A reference to the first element
         */

        constexpr const T& front()const{
            if(LEN == 0){
                throw std::out_of_range("Exception: array index out of bounds: cannot refernece front of array of size " + std::to_string(LEN));
            }
            return internal.front();
        }

        /**
         * \fn constexpr T& front()
         * \brief Returns a reference to the first element
         * \return A reference to the first element
         */

        constexpr T& front(){
            if(LEN == 0){
                throw std::out_of_range("Exception: array index out of bounds: cannot refernece front of array of size " + std::to_string(LEN));
            }
            return internal.front();
        }

        /**
         * \fn constexpr const T& back()const
         * \brief Returns a reference to the last element
         * \return A reference to the last element
         */

        constexpr const T& back()const{
            if(LEN == 0){
                throw std::out_of_range("Exception: array index out of bounds: cannot refernece back of array of size " + std::to_string(LEN));
            }
            return internal.back();
        }

        /**
         * \fn constexpr T& back()
         * \brief Returns a reference to the last element
         * \return A reference to the last element
         */

        constexpr T& back(){
            if(LEN == 0){
                throw std::out_of_range("Exception: array index out of bounds: cannot refernece back of array of size " + std::to_string(LEN));
            }
            return internal.back();
        }

        /**
         * \fn constexpr const std::array<T, LEN>& toArray()const
         * \brief Fetches the internal std::array of the BoundsCheckedArray
         * \return The internal std::array of this BoundsCheckedArray
         */

        constexpr const std::array<T, LEN>& toArray()const{
            return internal;
        }

        /**
         * \fn constexpr std::array<T, LEN>& toArray()
         * \brief Fetches the internal std::array of the BoundsCheckedArray
         * \return The internal std::array of this BoundsCheckedArray
         */

        constexpr std::array<T, LEN>*& toArray(){
            return internal;
        }

        /**
         * \fn constexpr const T* data()
         * \brief Fetches the underlying basic array
         * \return A pointer to the underlying basic array
         */

        constexpr const T* data()const{
            if(LEN == 0){
                return nullptr;
            }
            return internal.data();
        }

        /**
         * \fn constexpr const T* data()
         * \brief Fetches the underlying basic array
         * \return A pointer to the underlying basic array
         */

        constexpr T* data(){
            if(LEN == 0){
                return nullptr;
            }
            return internal.data();
        }

        /**
         * \fn constexpr std::size_t size()const
         * \brief Fetches the number of elements in the array
         * \return The number of elements in the array
         */

        constexpr std::size_t size()const{
            return internal.size();
        }

        /**
         * \fn constexpr bool empty()const
         * \brief Checks if the container has no elements
         * \return true if the array is empty, false otherwise
         */

        constexpr bool empty()const{
            return internal.empty();
        }

        /**
         * \fn constexpr std::size_t max_size()const
         * \brief Returns the maximum number of elements the array can hold
         * \return Maximum number of elements
         */

        constexpr std::size_t max_size()const{
            return internal.max_size();
        }

        /**
         * \fn constexpr typename std::array<T, LEN>::iterator begin()
         * \brief Returns an iterator to the first element in the container
         * \return Iterator to the first element
         */

        constexpr typename std::array<T, LEN>::iterator begin(){
            return internal.begin();
        }

        /**
         * \fn constexpr const typename std::array<T, LEN>::iterator begin()const
         * \brief Returns an iterator to the first element in the container
         * \return Iterator to the first element
         */

        constexpr const typename std::array<T, LEN>::iterator begin()const{
            return internal.begin();
        }

        /**
         * \fn constexpr typename std::array<T, LEN>::iterator end()
         * \brief Returns an iterator to the last element in the container
         * \return Iterator to the last element
         */

        constexpr typename std::array<T, LEN>::iterator end(){
            return internal.end();
        }

        /**
         * \fn constexpr const typename std::array<T, LEN>::iterator end()const
         * \brief Returns an iterator to the last element in the container
         * \return Iterator to the last element
         */

        constexpr const typename std::array<T, LEN>::iterator end()const{
            return internal.end();
        }

        BoundsCheckedArray(){}

        BoundsCheckedArray(std::initializer_list<T> list){
            if(list.size() != LEN){
                throw std::out_of_range("Exception: assignement to array of size " + std::to_string(LEN) + " to brace-enclosed initializer list of different size " + std::to_string(list.size()));
            }
            std::copy(list.begin(), list.end(), internal.begin());
        }

        template<typename S>
        BoundsCheckedArray(S iterable){
            if(iterable.size() != LEN){
                throw std::out_of_range("Exception: assignement to array of size " + std::to_string(LEN) + " to iterable of different size " + std::to_string(iterable.size()));
            }
            if(iterable.size() > 0 && !std::is_same<typename decltype(iterable)::value_type, T>::value){
                throw std::bad_cast();
            }
            std::copy(iterable.begin(), iterable.end(), internal.begin());
        }

        ~BoundsCheckedArray() = default;

        bool operator==(const BoundsCheckedArray<T, LEN>& b){
            return internal == b.internal;
        }

        bool operator!=(const BoundsCheckedArray<T, LEN>& b){
            return !(*this == b);
        }

        template<typename S, std::size_t L>
        friend std::string to_string(const BoundsCheckedArray<S, L>&, std::function<std::string(S)>);
    };

    template<typename T, std::size_t LEN>
    std::string to_string(const BoundsCheckedArray<T, LEN>& a, std::function<std::string(T)> to_s){
        return hel::to_string(a.internal, to_s);
    }
}

#endif
